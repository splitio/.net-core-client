using Splitio.CommonLibraries;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public class SplitioHttpClient : ISplitioHttpClient
    {
        private readonly ISplitLogger _log;
        private readonly HttpClient _httpClient;

        public SplitioHttpClient(
            string apiKey,
            long connectionTimeOut)
        {
#if NET40 || NET45
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
#endif
            _log = WrapperAdapter.GetLogger(typeof(SplitioHttpClient));
            _httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(connectionTimeOut)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
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
    }
}
