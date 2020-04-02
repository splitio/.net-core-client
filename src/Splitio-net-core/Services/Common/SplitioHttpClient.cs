using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public class SplitioHttpClient : ISplitioHttpClient
    {
        private readonly ISplitLogger _log;
        private readonly HttpClient _httpClient;

        public SplitioHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public SplitioHttpClient(
            string apiKey,
            long connectionTimeOut)
        {
#if NET40 || NET45
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)Constans.ProtocolTypeTls12;
#endif
            _log = WrapperAdapter.GetLogger(typeof(SplitioHttpClient));
            _httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(connectionTimeOut)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constans.Bearer, apiKey);
        }

        public async Task<HTTPResult> GetAsync(string url)
        {
            var result = new HTTPResult();

            try
            {
                using (var response = await _httpClient.GetAsync(new Uri(url)))
                {
                    result.statusCode = response.StatusCode;
                    result.content = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception caught executing GET {url}", e);
            }

            return result;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        public void Dispose()
        {
            _httpClient.CancelPendingRequests();
            _httpClient.Dispose();
        }        
    }
}
