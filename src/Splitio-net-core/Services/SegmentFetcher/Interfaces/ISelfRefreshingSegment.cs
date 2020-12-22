using System.Threading.Tasks;

namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISelfRefreshingSegment
    {
        Task FetchSegment();
    }
}
