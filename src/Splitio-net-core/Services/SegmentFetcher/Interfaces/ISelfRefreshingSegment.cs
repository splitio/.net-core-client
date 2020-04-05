namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISelfRefreshingSegment
    {
        void FetchSegment(string name);
        void FetchSegment();
    }
}
