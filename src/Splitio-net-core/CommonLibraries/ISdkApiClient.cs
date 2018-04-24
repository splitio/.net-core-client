using System.Threading.Tasks;

namespace Splitio.CommonLibraries
{
    public interface ISdkApiClient
    {
        Task<HTTPResult> ExecuteGet(string requestUri);

        Task<HTTPResult> ExecutePost(string requestUri, string data);
    }
}
