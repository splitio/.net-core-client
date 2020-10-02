using Newtonsoft.Json;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Splitio.Services.Impressions.Classes
{
    public class TreatmentSdkApiClient : SdkApiClient, ITreatmentSdkApiClient
    {
        private const string TestImpressionsUrlTemplate = "/api/testImpressions/bulk";
        private const string ImpressionsCountUrlTemplate = "/api/testImpressions/count";

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

        public async void SendImpressionsCount(ConcurrentDictionary<KeyCache, int> impressionsCount)
        {
            var json = ConvertToJson(impressionsCount);

            var response = await ExecutePost(ImpressionsCountUrlTemplate, json);

            if ((int)response.statusCode < (int)HttpStatusCode.OK || (int)response.statusCode >= (int)HttpStatusCode.Ambiguous)
            {
                Log.Error(string.Format("Http status executing SendBulkImpressions: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }

        // Public for tests
        public string ConvertToJson(List<KeyImpression> impressions)
        {
            var impressionsPerFeature =
                impressions
                .GroupBy(item => item.feature)
                .Select(group => new { f = group.Key, i = group.Select(x => new { k = x.keyName, t = x.treatment, m = x.time, c = x.changeNumber, r = x.label, b = x.bucketingKey }) });

            return JsonConvert.SerializeObject(impressionsPerFeature);
        }

        public string ConvertToJson(ConcurrentDictionary<KeyCache, int> impressionsCount)
        {
            return JsonConvert.SerializeObject(new
            {
                pf = impressionsCount.Select(item => new
                {
                    f = item.Key.SplitName,
                    m = item.Key.TimeFrame,
                    rc = item.Value
                })
            });
        }
    }
}
