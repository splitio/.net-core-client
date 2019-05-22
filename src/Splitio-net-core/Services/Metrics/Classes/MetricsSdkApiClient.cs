using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Services.Metrics.Interfaces;
using System.Net;

namespace Splitio.Services.Metrics.Classes
{
    public class MetricsSdkApiClient : SdkApiClient, IMetricsSdkApiClient
    {
        private const string MetricsUrlTemplate = "/api/metrics/{endpoint}";

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetricsSdkApiClient));

        public MetricsSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout) 
            : base(header, baseUrl, connectionTimeOut, readTimeout)
        { }

        public async void SendCountMetrics(string metrics)
        {
            var response = await ExecutePost(MetricsUrlTemplate.Replace("{endpoint}", "counters"), metrics);

            if ((int)response.statusCode >= (int)HttpStatusCode.OK && (int)response.statusCode < (int)HttpStatusCode.Ambiguous)
            {
                Log.Error(string.Format("Http status executing SendCountMetrics: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }

        public async void SendTimeMetrics(string metrics)
        {
            var response = await ExecutePost(MetricsUrlTemplate.Replace("{endpoint}", "times"), metrics);

            if ((int)response.statusCode >= (int)HttpStatusCode.OK && (int)response.statusCode < (int)HttpStatusCode.Ambiguous)
            {
                Log.Error(string.Format("Http status executing SendTimeMetrics: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }

        public async void SendGaugeMetrics(string metrics)
        {
            var response = await ExecutePost(MetricsUrlTemplate.Replace("{endpoint}", "gauge"), metrics);

            if ((int)response.statusCode >= (int)HttpStatusCode.OK && (int)response.statusCode < (int)HttpStatusCode.Ambiguous)
            {
                Log.Error(string.Format("Http status executing SendGaugeMetrics: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }
    }
}
