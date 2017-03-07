using log4net;
using Newtonsoft.Json;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Metrics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Metrics.Classes
{
    public class InMemoryMetricsLog : IMetricsLog
    {
        IMetricsSdkApiClient apiClient;
        IMetricsCache metricsCache;
        private int maxCountCalls;
        private int maxTimeBetweenCalls;
        private DateTime utcNowTimestamp = DateTime.UtcNow;
        private DateTime countLastCall;
        private DateTime timeLastCall;
        private DateTime gaugeLastCall;
        private object countMetricsLockObject = new Object();
        private object timeMetricsLockObject = new Object();
        private object gaugeMetricsLockObject = new Object();
        private Boolean sendingCountMetrics = false;
        private Boolean sendingTimeMetrics = false;
        private Boolean sendingGaugeMetrics = false;
        private int gaugeCallCount = 0;


        protected static readonly ILog Logger = LogManager.GetLogger(typeof(InMemoryMetricsLog));

        public InMemoryMetricsLog(IMetricsSdkApiClient apiClient, IMetricsCache metricsCache, int maxCountCalls = 1000, int maxTimeBetweenCalls = 60)
        {
            this.apiClient = apiClient;
            this.metricsCache = metricsCache;
            this.maxCountCalls = maxCountCalls;
            this.maxTimeBetweenCalls = maxTimeBetweenCalls * 1000;
            this.countLastCall = utcNowTimestamp;
            this.timeLastCall = utcNowTimestamp;
            this.gaugeLastCall = utcNowTimestamp;
        }

        public void Count(string counterName, long delta)
        {
            if (string.IsNullOrEmpty(counterName) ||  delta <= 0)
            {
                return;
            }
           
            Counter counter = metricsCache.IncrementCount(counterName, delta);

            var oldLastCall = countLastCall;
            countLastCall = DateTime.UtcNow;
            if (counter.GetCount() >= maxCountCalls || ((countLastCall - oldLastCall).TotalMilliseconds > maxTimeBetweenCalls))
            {
                SendCountMetrics();
            }
        }

        public void Time(string operation, long miliseconds)
        {
            if (string.IsNullOrEmpty(operation) || miliseconds < 0)
            {
                return;
            }
          
            metricsCache.SetLatency(operation, miliseconds);

            var oldLastCall = timeLastCall;
            timeLastCall = DateTime.UtcNow;
            if ((timeLastCall - oldLastCall).TotalMilliseconds > maxTimeBetweenCalls)
            {
                SendTimeMetrics();
            }
        }

        public void Gauge(string gauge, long value)
        {
            if (string.IsNullOrEmpty(gauge) || value < 0)
            {
                return;
            }

            metricsCache.SetGauge(gauge, value);
            gaugeCallCount++;

            var oldLastCall = gaugeLastCall;
            gaugeLastCall = DateTime.UtcNow;
            if (gaugeCallCount >= maxCountCalls || (gaugeLastCall - oldLastCall).TotalMilliseconds > maxTimeBetweenCalls)
            {
                SendGaugeMetrics();
            }
        }


        private void SendCountMetrics()
        {
            lock (countMetricsLockObject)
            {
                if (sendingCountMetrics)
                {
                    return;
                }
                sendingCountMetrics = true;
            }

            var countMetricsJson = ConvertCountMetricsToJson(metricsCache.FetchAllCountersAndClear());

            if (countMetricsJson != string.Empty)
            {
                apiClient.SendCountMetrics(countMetricsJson);
            }
            sendingCountMetrics = false;
        }

        private string ConvertCountMetricsToJson(Dictionary<string, Counter> countMetrics)
        {
            try
            {
                return JsonConvert.SerializeObject(countMetrics.Select(x => new { name = x.Key, delta = x.Value.GetDelta() }));
            }
            catch(Exception e)
            {
                Logger.Error("Exception ocurred serializing count metrics", e);

                return string.Empty;
            }
        }
        private void SendTimeMetrics()
        {
            lock (timeMetricsLockObject)
            {
                if (sendingTimeMetrics)
                {
                    return;
                }
                sendingTimeMetrics = true;
            }
            var timeMetricsJson = ConvertTimeMetricsToJson(metricsCache.FetchAllLatencyTrackersAndClear());
            if (timeMetricsJson != string.Empty)
            {
                apiClient.SendTimeMetrics(timeMetricsJson);
            }
            sendingTimeMetrics = false;
        }

        private string ConvertTimeMetricsToJson(Dictionary<string, ILatencyTracker> timeMetrics)
        {
            try
            {
                return JsonConvert.SerializeObject(timeMetrics.Select(x => new { name = x.Key, latencies = x.Value.GetLatencies()}));
            }
            catch (Exception e)
            {
                Logger.Error("Exception ocurred serializing time metrics", e);

                return string.Empty;
            }
        }

        private void SendGaugeMetrics()
        {
            lock (gaugeMetricsLockObject)
            {
                if (sendingGaugeMetrics)
                {
                    return;
                }
                sendingGaugeMetrics = true;
            }

            var gaugeMetricsJson = ConvertGaugeMetricsToJson(metricsCache.FetchAllGaugesAndClear());
            gaugeCallCount = 0;
            if (gaugeMetricsJson != string.Empty)
            {
                apiClient.SendGaugeMetrics(gaugeMetricsJson);
            }
            sendingGaugeMetrics = false;
        }

        private string ConvertGaugeMetricsToJson(Dictionary<string, long> gaugeMetrics)
        {
            try
            {
                return JsonConvert.SerializeObject(gaugeMetrics.Select(x => new { name = x.Key, value = x.Value }));
            }
            catch (Exception e)
            {
                Logger.Error("Exception ocurred serializing gauge metrics", e);

                return string.Empty;
            }
        }
    }
}
