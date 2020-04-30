namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISelfRefreshingSegmentFetcher
    {
        void Start();
        void Stop();
        void FetchAll();
        void Fetch(string segmentName);
        void Clear();
    }
}
