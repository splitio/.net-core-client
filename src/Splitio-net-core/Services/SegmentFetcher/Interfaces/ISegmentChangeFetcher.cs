using Splitio.Domain;

namespace Splitio.Services.SegmentFetcher.Interfaces
{
    public interface ISegmentChangeFetcher
    {
        SegmentChange Fetch(string name, long change_number);
    }
}
