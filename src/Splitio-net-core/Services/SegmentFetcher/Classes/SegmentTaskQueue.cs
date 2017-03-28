using System.Collections.Concurrent;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public static class SegmentTaskQueue
    {
        //ConcurrentQueue<T> performs best when one dedicated thread is queuing and one dedicated thread is de-queuing
        public static BlockingCollection<SelfRefreshingSegment> segmentsQueue = new BlockingCollection<SelfRefreshingSegment>(new ConcurrentQueue<SelfRefreshingSegment>());
    }
}
