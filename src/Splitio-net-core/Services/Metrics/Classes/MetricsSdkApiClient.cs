using NLog;
using Splitio.CommonLibraries;
using Splitio.Services.Metrics.Interfaces;
using System.Net;

namespace Splitio.Services.Metrics.Classes
{
    public class MetricsSdkApiClient : SdkApiClient, IMetricsSdkApiClient
    {
        private const string MetricsUrlTemplate = "/api/metrics/{endpoint}";

        private static readonly Logger Log = LogManager.GetLogger(typeof(MetricsSdkApiClient).ToString());

        public MetricsSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout) : base(header, baseUrl, connectionTimeOut, readTimeout) { }

        public void SendCountMetrics(string metrics)
        {
            var response = ExecutePost(MetricsUrlTemplate.Replace("{endpoint}", "counters"), metrics);
            if (response.statusCode != HttpStatusCode.OK)
            {
                Log.Error(string.Format("Http status executing SendCountMetrics: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }

        public void SendTimeMetrics(string metrics)
        {
            var response = ExecutePost(MetricsUrlTemplate.Replace("{endpoint}", "times"), metrics);
            if (response.statusCode != HttpStatusCode.OK)
            {
                Log.Error(string.Format("Http status executing SendTimeMetrics: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }

        public void SendGaugeMetrics(string metrics)
        {
            var response = ExecutePost(MetricsUrlTemplate.Replace("{endpoint}", "gauge"), metrics);
            if (response.statusCode != HttpStatusCode.OK)
            {
                Log.Error(string.Format("Http status executing SendGaugeMetrics: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }
    }
}
