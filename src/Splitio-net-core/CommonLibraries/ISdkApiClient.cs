
namespace Splitio.CommonLibraries
{
    public interface ISdkApiClient
    {
        HTTPResult ExecuteGet(string requestUri);

        HTTPResult ExecutePost(string requestUri, string data);
    }
}
