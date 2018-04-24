using Splitio.Domain;
using System.Threading.Tasks;

namespace Splitio.Services.SplitFetcher.Interfaces
{
    public interface ISplitChangeFetcher
    {
        Task<SplitChangesResult> Fetch(long since);
    }
}
