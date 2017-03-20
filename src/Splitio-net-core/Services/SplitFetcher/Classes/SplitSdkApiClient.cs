using log4net;
using Splitio.CommonLibraries;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Diagnostics;
using System.Net;

namespace Splitio.Services.SplitFetcher.Classes
{
    public class SplitSdkApiClient : SdkApiClient, ISplitSdkApiClient
    {
        private const string SplitChangesUrlTemplate = "/api/splitChanges";
        private const string UrlParameterSince = "?since=";
        private const string SplitFetcherTime = "splitChangeFetcher.time";
        private const string SplitFetcherStatus = "splitChangeFetcher.status.{0}";
        private const string SplitFetcherException = "splitChangeFetcher.exception";

        private static readonly ILog Log = LogManager.GetLogger("splitio",typeof(SplitSdkApiClient));

        public SplitSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout, IMetricsLog metricsLog = null) : base(header, baseUrl, connectionTimeOut, readTimeout, metricsLog) { }

        public string FetchSplitChanges(long since)
        {
            var clock = new Stopwatch();
            clock.Start();
            try
            {
                var requestUri = GetRequestUri(since);
                var response = ExecuteGet(requestUri);
                if (response.statusCode == HttpStatusCode.OK)
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
