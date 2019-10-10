using Splitio.CommonLibraries;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Splitio.Services.SplitFetcher.Classes
{
    public class SplitSdkApiClient : SdkApiClient, ISplitSdkApiClient
    {
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(SplitSdkApiClient));

        private const string SplitChangesUrlTemplate = "/api/splitChanges";
        private const string UrlParameterSince = "?since=";
        private const string SplitFetcherTime = "splitChangeFetcher.time";
        private const string SplitFetcherStatus = "splitChangeFetcher.status.{0}";
        private const string SplitFetcherException = "splitChangeFetcher.exception";
        
        public SplitSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout, IMetricsLog metricsLog = null) : base(header, baseUrl, connectionTimeOut, readTimeout, metricsLog) { }

        public async Task<string> FetchSplitChanges(long since)
        {
            var clock = new Stopwatch();
            clock.Start();

            try
            {
                var requestUri = GetRequestUri(since);
                var response = await ExecuteGet(requestUri);

                if ((int)response.statusCode >= (int)HttpStatusCode.OK && (int)response.statusCode < (int)HttpStatusCode.Ambiguous)
                {
                    if (metricsLog != null)
                    {
                        metricsLog.Time(SplitFetcherTime, clock.ElapsedMilliseconds);
                        metricsLog.Count(string.Format(SplitFetcherStatus, response.statusCode), 1);
                    }

                    return response.content;
                }
                else
                {
                    Log.Error(string.Format("Http status executing FetchSplitChanges: {0} - {1}", response.statusCode.ToString(), response.content));

                    if (metricsLog != null)
                    {
                        metricsLog.Count(string.Format(SplitFetcherStatus, response.statusCode), 1);
                    }

                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception caught executing FetchSplitChanges", e);

                if (metricsLog != null)
                {
                    metricsLog.Count(SplitFetcherException, 1);
                }

                return string.Empty;
            }
        }

        private string GetRequestUri(long since)
        {
            return string.Concat(SplitChangesUrlTemplate, UrlParameterSince, Uri.EscapeDataString(since.ToString()));
        }
    }
}
