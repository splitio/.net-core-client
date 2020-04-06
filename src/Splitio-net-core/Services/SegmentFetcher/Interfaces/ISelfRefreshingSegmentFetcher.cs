namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISelfRefreshingSegmentFetcher
    {
        void Start();
        void Stop();
        void FetchSegments();
        void FetchSegment(string segmentName);
    }
}
