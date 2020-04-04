using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource.Workers
{
    public class SegmentsWorker : ISegmentsWorker
    {
        private readonly ISplitLogger _log;
        private readonly ISegmentCache _segmentCache;
        private readonly ISegmentChangeFetcher _segmentChangeFetcher;
        private readonly IReadinessGatesCache _gates;
        private readonly BlockingCollection<SegmentQueueDto> _queue;

        private CancellationTokenSource _cancellationTokenSource;

        public SegmentsWorker(ISegmentCache segmentCache,
            ISegmentChangeFetcher segmentChangeFetcher,
            IReadinessGatesCache gates,
            ISplitLogger log = null)
        {
            _segmentCache = segmentCache;
            _segmentChangeFetcher = segmentChangeFetcher;
            _gates = gates;
            _log = log ?? WrapperAdapter.GetLogger(typeof(SegmentsWorker));
            _queue = new BlockingCollection<SegmentQueueDto>(new ConcurrentQueue<SegmentQueueDto>());
        }

        #region Public Methods
        public void AddToQueue(long changeNumber, string segmentName)
        {
            _queue.TryAdd(new SegmentQueueDto
            {
                ChangeNumber = changeNumber,
                SegmentName = segmentName
            });
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => Execute(), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
        #endregion

        #region Private Methods
        public void Execute()
        {
            while (true)
            {
                //Wait indefinitely until a segment is queued
                if (_queue.TryTake(out SegmentQueueDto segment, -1))
                {
                    _log.Debug($"Segment dequeue: {segment.SegmentName}");

                    if (segment.ChangeNumber > _segmentCache.GetChangeNumber(segment.SegmentName))
                    {
                        // TODO: change this after synchronizer implementation.
                        var selfRefreshingSegment = new SelfRefreshingSegment(segment.SegmentName, _segmentChangeFetcher, _gates, _segmentCache);
                        selfRefreshingSegment.RefreshSegment();
                    }
                }
            }
        }
        #endregion
    }
}
