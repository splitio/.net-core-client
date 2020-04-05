using Splitio.CommonLibraries;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public class SelfRefreshingSegmentFetcher : SegmentFetcher, ISelfRefreshingSegmentFetcher
    {
        private static readonly ISplitLogger _log = WrapperAdapter.GetLogger(typeof(SelfRefreshingSegmentFetcher));

        private readonly ISegmentChangeFetcher _segmentChangeFetcher;
        private readonly IReadinessGatesCache _gates;
        private readonly IWrapperAdapter _wrappedAdapter;
        private readonly ConcurrentDictionary<string, SelfRefreshingSegment> _segments;
        private readonly SegmentTaskWorker _worker;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly int _interval;

        public SelfRefreshingSegmentFetcher(ISegmentChangeFetcher segmentChangeFetcher, 
            IReadinessGatesCache gates, 
            int interval, 
            ISegmentCache segmentsCache, 
            int numberOfParallelSegments) : base(segmentsCache)
        {
            _cancelTokenSource = new CancellationTokenSource();

            _segmentChangeFetcher = segmentChangeFetcher;
            _segments = new ConcurrentDictionary<string, SelfRefreshingSegment>();
            _worker = new SegmentTaskWorker(numberOfParallelSegments);
            _interval = interval;
            _gates = gates;
            _wrappedAdapter = new WrapperAdapter();

            StartWorker();
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
            SegmentTaskQueue.segmentsQueue.Dispose();
            _segments.Clear();
            _segmentCache.Clear();
        }

        public void Start()
        {
            //Delay first execution until expected time has passed
            _wrappedAdapter.TaskDelay(_interval * 1000).Wait();

            var schedulerTask = PeriodicTaskFactory.Start(() => AddSegmentsToQueue(), intervalInMilliseconds: _interval * 1000, cancelToken: _cancelTokenSource.Token);
        }

        public override void InitializeSegment(string name)
        {
            _segments.TryGetValue(name, out SelfRefreshingSegment segment);

            if (segment == null)
            {
                segment = new SelfRefreshingSegment(name, _segmentChangeFetcher, _gates, _segmentCache);

                _segments.TryAdd(name, segment);

                SegmentTaskQueue.segmentsQueue.TryAdd(segment);

                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"Segment queued: {segment.Name}");
                }
            }
        }

        private void AddSegmentsToQueue()
        {
            foreach (var segment in _segments.Values)
            {
                SegmentTaskQueue.segmentsQueue.TryAdd(segment);

                if (_log.IsDebugEnabled)
                {
                    _log.Debug(string.Format("Segment queued: {0}", segment.Name));
                }
            }
        }

        private void StartWorker()
        {
            var workerTask = Task.Factory.StartNew(() => _worker.ExecuteTasks(_cancelTokenSource.Token), _cancelTokenSource.Token);
        }
    }
}
