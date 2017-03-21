using NLog;
using Splitio.Services.Metrics.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Splitio.CommonLibraries
{
    public class SdkApiClient : ISdkApiClient
    {
        private HttpClient httpClient;
        private static readonly Logger Log = LogManager.GetLogger(typeof(SdkApiClient).ToString());
        protected IMetricsLog metricsLog;

        public SdkApiClient (HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout, IMetricsLog metricsLog = null)
        {
            if (header.encoding == "gzip")
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                httpClient = new HttpClient(handler);
            }
            else
            {
                httpClient = new HttpClient();
            }

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
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", header.encoding);
            httpClient.DefaultRequestHeaders.Add("Keep-Alive", "true");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //TODO: find a way to store it in sepparated parameters
            httpClient.Timeout = TimeSpan.FromMilliseconds((connectionTimeOut + readTimeout));

            this.metricsLog = metricsLog;
        }

        public virtual HTTPResult ExecuteGet(string requestUri)
        {
            var result = new HTTPResult();
            try
            {
                var task = httpClient.GetAsync(requestUri);
                task.Wait();
                var response = task.Result;
                result.statusCode = response.StatusCode;
                result.content = response.Content.ReadAsStringAsync().Result;                
            }
            catch(Exception e)
            {
                Log.Error(e, string.Format("Exception caught executing GET {0}", requestUri));
            }
            return result;
        }

        public virtual HTTPResult ExecutePost(string requestUri, string data)
        {
            var result = new HTTPResult();
            try
            {
                var task = httpClient.PostAsync(requestUri, new StringContent(data, Encoding.UTF8, "application/json"));
                task.Wait();
                var response = task.Result;
                result.statusCode = response.StatusCode;
                result.content = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Log.Error(e, string.Format("Exception caught executing POST {0}", requestUri));
            }
            return result;
        }


    }
}
