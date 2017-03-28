using NLog;
using Splitio.CommonLibraries;
using Splitio.Services.Impressions.Interfaces;
using System.Net;

namespace Splitio.Services.Impressions.Classes
{
    public class TreatmentSdkApiClient : SdkApiClient, ITreatmentSdkApiClient
    {
        private const string TestImpressionsUrlTemplate = "/api/testImpressions/bulk";
        
        private static readonly Logger Log = LogManager.GetLogger(typeof(TreatmentSdkApiClient).ToString());

        public TreatmentSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout) : base(header, baseUrl, connectionTimeOut, readTimeout) { }

        public void SendBulkImpressions(string impressions)
        {
            var response = ExecutePost(TestImpressionsUrlTemplate, impressions);
            if (response.statusCode != HttpStatusCode.OK)
            {
                Log.Error(string.Format("Http status executing SendBulkImpressions: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }
    }
}
