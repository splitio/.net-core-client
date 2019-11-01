using Newtonsoft.Json;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Splitio.Services.Impressions.Classes
{
    public class TreatmentSdkApiClient : SdkApiClient, ITreatmentSdkApiClient
    {
        private const string TestImpressionsUrlTemplate = "/api/testImpressions/bulk";
        
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(TreatmentSdkApiClient));

        public TreatmentSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout) 
            : base(header, baseUrl, connectionTimeOut, readTimeout)
        { }

        public async void SendBulkImpressions(List<KeyImpression> impressions)
        {
            var impressionsJson = ConvertToJson(impressions);

            var response = await ExecutePost(TestImpressionsUrlTemplate, impressionsJson);

            if ((int)response.statusCode < (int)HttpStatusCode.OK || (int)response.statusCode >= (int)HttpStatusCode.Ambiguous)
            {
                Log.Error(string.Format("Http status executing SendBulkImpressions: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }

        private string ConvertToJson(List<KeyImpression> impressions)
        {
            var impressionsPerFeature =
                impressions
                .GroupBy(item => item.feature)
                .Select(group => new { testName = group.Key, keyImpressions = group.Select(x => new { keyName = x.keyName, treatment = x.treatment, time = x.time, changeNumber = x.changeNumber, label = x.label, bucketingKey = x.bucketingKey }) });

            return JsonConvert.SerializeObject(impressionsPerFeature);
        }
    }
}
