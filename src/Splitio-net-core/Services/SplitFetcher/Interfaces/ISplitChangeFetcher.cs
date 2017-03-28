using Splitio.Domain;

namespace Splitio.Services.SplitFetcher.Interfaces
{
    public interface ISplitChangeFetcher
    {
        SplitChangesResult Fetch(long since);
    }
}
