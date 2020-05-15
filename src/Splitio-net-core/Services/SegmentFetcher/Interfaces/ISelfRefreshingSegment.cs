using System.Threading.Tasks;

namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISelfRefreshingSegment
    {
        Task FetchSegment(string name);
        Task FetchSegment();
    }
}
