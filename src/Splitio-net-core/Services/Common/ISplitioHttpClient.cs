using Splitio.CommonLibraries;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public interface ISplitioHttpClient
    {
        Task<HTTPResult> GetAsync(string url);
    }
}
