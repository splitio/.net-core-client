using Common.Logging;
using Splitio.Services.Metrics.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Splitio.CommonLibraries
{
    public class SdkApiClient : ISdkApiClient
    {
        private HttpClient httpClient;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SdkApiClient));
        protected IMetricsLog metricsLog;

        public SdkApiClient (HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout, IMetricsLog metricsLog = null)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            httpClient = new HttpClient(handler);

            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", header.authorizationApiKey);
            httpClient.DefaultRequestHeaders.Add("SplitSDKVersion", header.splitSDKVersion);
            httpClient.DefaultRequestHeaders.Add("SplitSDKSpecVersion", header.splitSDKSpecVersion);
            if (!string.IsNullOrEmpty(header.splitSDKMachineName))
            {
                httpClient.DefaultRequestHeaders.Add("SplitSDKMachineName", header.splitSDKMachineName);
            }
            if (!string.IsNullOrEmpty(header.splitSDKMachineIP))
            {
                httpClient.DefaultRequestHeaders.Add("SplitSDKMachineIP", header.splitSDKMachineIP);
            }
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //TODO: find a way to store it in sepparated parameters
            httpClient.Timeout = TimeSpan.FromMilliseconds((connectionTimeOut + readTimeout));

            this.metricsLog = metricsLog;
        }

        public virtual async Task<HTTPResult> ExecuteGet(string requestUri)
        {
            var result = new HTTPResult();
            try
            {
                using (var response = await httpClient.GetAsync(requestUri))
                {
                    result.statusCode = response.StatusCode;
                    result.content = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch(Exception e)
            {
                Log.Error(string.Format("Exception caught executing GET {0}", requestUri), e);
            }
            return result;
        }

        public virtual async Task<HTTPResult> ExecutePost(string requestUri, string data)
        {
            var result = new HTTPResult();
            try
            {
                using (var response = await httpClient.PostAsync(requestUri, new StringContent(data, Encoding.UTF8, "application/json")))
                {
                    result.statusCode = response.StatusCode;
                    result.content = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                Log.Error(string.Format("Exception caught executing POST {0}", requestUri), e);
            }
            return result;
        }


    }
}
