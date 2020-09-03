using Splitio.Services.SegmentFetcher.Interfaces;
using System.Collections.Concurrent;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public class SegmentTaskQueue : ISegmentTaskQueue
    {
        private readonly BlockingCollection<SelfRefreshingSegment> _segmentsQueue;

        public SegmentTaskQueue()
        {
            _segmentsQueue = new BlockingCollection<SelfRefreshingSegment>(new ConcurrentQueue<SelfRefreshingSegment>());
        }

        public void Dispose()
        {
            _segmentsQueue.Dispose();
        }

        public void Add(SelfRefreshingSegment selfRefreshingSegment)
        {
            _segmentsQueue.TryAdd(selfRefreshingSegment);
        }

        public BlockingCollection<SelfRefreshingSegment> GetQueue()
        {
            return _segmentsQueue;
        }
    }
}
