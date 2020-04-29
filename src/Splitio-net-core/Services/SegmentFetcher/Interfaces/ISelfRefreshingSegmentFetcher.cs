namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISelfRefreshingSegmentFetcher
    {
        void Start();
        void Stop(bool isDestroy = false);
        void FetchAll();
        void Fetch(string segmentName);
    }
}
