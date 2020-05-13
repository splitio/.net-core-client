using System.Threading.Tasks;

namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISelfRefreshingSegmentFetcher
    {
        void Start();
        void Stop();
        Task FetchAll();
        Task Fetch(string segmentName);
        void Clear();
    }
}
