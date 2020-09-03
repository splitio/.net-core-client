using Splitio.Services.SegmentFetcher.Classes;
using System;
using System.Collections.Concurrent;

namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISegmentTaskQueue : IDisposable
    {
        void Add(SelfRefreshingSegment selfRefreshingSegment);
        BlockingCollection<SelfRefreshingSegment> GetQueue();
    }
}
