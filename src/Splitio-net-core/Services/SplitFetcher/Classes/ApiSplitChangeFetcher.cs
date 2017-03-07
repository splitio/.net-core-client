using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Services.SplitFetcher.Interfaces;

namespace Splitio.Services.SplitFetcher.Classes
{
    public class ApiSplitChangeFetcher: SplitChangeFetcher, ISplitChangeFetcher 
    {
        private readonly ISplitSdkApiClient apiClient;

        public ApiSplitChangeFetcher(ISplitSdkApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        protected override SplitChangesResult FetchFromBackend(long since)
        {
            var fetchResult = apiClient.FetchSplitChanges(since);

            var splitChangesResult = JsonConvert.DeserializeObject<SplitChangesResult>(fetchResult);
            return splitChangesResult;
        }
    }
}
