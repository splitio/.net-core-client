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
        private readonly ISegmentTaskQueue _segmentTaskQueue;
        private readonly ConcurrentDictionary<string, SelfRefreshingSegment> _segments;
        private readonly SegmentTaskWorker _worker;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly int _interval;

        public SelfRefreshingSegmentFetcher(ISegmentChangeFetcher segmentChangeFetcher, 
            IReadinessGatesCache gates, 
            int interval, 
            ISegmentCache segmentsCache, 
            int numberOfParallelSegments,
            ISegmentTaskQueue segmentTaskQueue) : base(segmentsCache)
        {
            _cancelTokenSource = new CancellationTokenSource();

            _segmentChangeFetcher = segmentChangeFetcher;
            _segments = new ConcurrentDictionary<string, SelfRefreshingSegment>();
            _worker = new SegmentTaskWorker(numberOfParallelSegments, segmentTaskQueue);
            _interval = interval;
            _gates = gates;
            _wrappedAdapter = new WrapperAdapter();
            _segmentTaskQueue = segmentTaskQueue;

            StartWorker();
        }

        #region Public Methods
        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_gates.IsSDKReady(0))
                    {
                        //Delay first execution until expected time has passed
                        var intervalInMilliseconds = _interval * 1000;
                        _wrappedAdapter.TaskDelay(intervalInMilliseconds).Wait();

                        PeriodicTaskFactory.Start(() => AddSegmentsToQueue(), intervalInMilliseconds, _cancelTokenSource.Token);
                        break;
                    }

                    _wrappedAdapter.TaskDelay(500).Wait();
                }
            });
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
        }

        public void Clear()
        {
            _segmentTaskQueue.Dispose();
            _segments.Clear();
            _segmentCache.Clear();
        }

        public override void InitializeSegment(string name)
        {
            _segments.TryGetValue(name, out SelfRefreshingSegment segment);

            if (segment == null)
            {
                segment = new SelfRefreshingSegment(name, _segmentChangeFetcher, _gates, _segmentCache);

                _segments.TryAdd(name, segment);

                _segmentTaskQueue.Add(segment);

                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"Segment queued: {segment.Name}");
                }
            }
        }

        public async Task FetchAll()
        {
            foreach (var segment in _segments.Values)
            {
                await segment.FetchSegment();

                _log.Debug(string.Format("Segment fetched: {0}", segment.Name));
            }
        }

        public async Task Fetch(string segmentName)
        {
            var refreshingSegment = new SelfRefreshingSegment(segmentName, _segmentChangeFetcher, _gates, _segmentCache);
            await refreshingSegment.FetchSegment();
        }
        #endregion

        #region Private Methods
        private void AddSegmentsToQueue()
        {
            foreach (var segment in _segments.Values)
            {
                _segmentTaskQueue.Add(segment);

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
        #endregion
    }
}
