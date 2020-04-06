using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.Shared.Classes;
using System;
using System.Threading.Tasks;

namespace Splitio.Services.Metrics.Classes
{
    public class AsyncMetricsLog : IMetricsLog
    {
        private readonly IMetricsSdkApiClient _metricsSdkApiClient;
        private readonly IMetricsCache _metricsCache;
        private readonly int _maxCountCalls;
        private readonly int _maxTimeBetweenCalls;

        IMetricsLog worker;

        protected static readonly ISplitLogger Logger = WrapperAdapter.GetLogger(typeof(AsyncMetricsLog));

        public AsyncMetricsLog(IMetricsSdkApiClient metricsSdkApiClient,
            IMetricsCache metricsCache,
            int maxCountCalls = -1,
            int maxTimeBetweenCalls = -1)
        {
            _metricsSdkApiClient = metricsSdkApiClient;
            _metricsCache = metricsCache;
            _maxCountCalls = maxCountCalls;
            _maxTimeBetweenCalls = maxTimeBetweenCalls;
        }

        public void Start()
        {
            worker = new InMemoryMetricsLog(_metricsSdkApiClient, _metricsCache, _maxCountCalls, _maxTimeBetweenCalls);
        }

        public void Count(string counter, long delta)
        {
            try
            {
                var task = new Task(() => worker.Count(counter, delta));
                task.Start();
            }
            catch(Exception e)
            {
                Logger.Error("Exception running count metrics task", e);
            }
        }

        public void Time(string operation, long miliseconds)
        {
            try
            {
                var task = new Task(() => worker.Time(operation, miliseconds));
                task.Start();
            }
            catch(Exception e)
            {
                Logger.Error("Exception running time metrics task", e);
            }
        }

        public void Gauge(string gauge, long value)
        {
            try
            {
                var task = new Task(() => worker.Gauge(gauge, value));
                task.Start();
            }
            catch(Exception e)
            {
                Logger.Error("Exception running gauge metrics task", e);
            }
        }

        public void Clear()
        {
            worker.Clear();
        }
    }
}
