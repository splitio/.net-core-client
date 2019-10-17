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
    public class SelfRefreshingSegmentFetcher : SegmentFetcher
    {
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(SelfRefreshingSegmentFetcher));

        private readonly ISegmentChangeFetcher segmentChangeFetcher;
        private readonly ConcurrentDictionary<string, SelfRefreshingSegment> segments;
        private readonly SegmentTaskWorker worker;
        private readonly IReadinessGatesCache gates;
        private readonly int interval;
        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private readonly IWrapperAdapter _wrappedAdapter;

        public SelfRefreshingSegmentFetcher(ISegmentChangeFetcher segmentChangeFetcher, IReadinessGatesCache gates, int interval, ISegmentCache segmentsCache, int numberOfParallelSegments) : base(segmentsCache)
        {
            this.segmentChangeFetcher = segmentChangeFetcher;
            this.segments = new ConcurrentDictionary<string, SelfRefreshingSegment>();
            worker = new SegmentTaskWorker(numberOfParallelSegments);
            this.interval = interval;
            this.gates = gates;
            _wrappedAdapter = new WrapperAdapter();

            StartWorker();
        }

        public void Stop()
        {
            cancelTokenSource.Cancel();
            SegmentTaskQueue.segmentsQueue.Dispose();
            segments.Clear();
            segmentCache.Clear();
        }

        private void StartWorker()
        {
            Task workerTask = Task.Factory.StartNew(
                () => worker.ExecuteTasks(cancelTokenSource.Token),
                cancelTokenSource.Token);
        }

        public void StartScheduler()
        {
            //Delay first execution until expected time has passed
            _wrappedAdapter.TaskDelay(interval * 1000).Wait();

            Task schedulerTask = PeriodicTaskFactory.Start(
                    () => AddSegmentsToQueue(),
                    intervalInMilliseconds: interval * 1000,
                    cancelToken: cancelTokenSource.Token);
        }

        public override void InitializeSegment(string name)
        {
            SelfRefreshingSegment segment;
            segments.TryGetValue(name, out segment);
            if (segment == null)
            {
                segment = new SelfRefreshingSegment(name, segmentChangeFetcher, gates, segmentCache);
                segments.TryAdd(name, segment);
                SegmentTaskQueue.segmentsQueue.TryAdd(segment);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format("Segment queued: {0}", segment.name));
                }
            }
        }

        private void AddSegmentsToQueue()
        {
            foreach (SelfRefreshingSegment segment in segments.Values)
            {
                SegmentTaskQueue.segmentsQueue.TryAdd(segment);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format("Segment queued: {0}", segment.name));
                }
            }
        }
    }
}
