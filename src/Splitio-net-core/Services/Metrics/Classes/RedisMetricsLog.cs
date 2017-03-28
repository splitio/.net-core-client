using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Metrics.Interfaces;

namespace Splitio.Services.Metrics.Classes
{
    public class RedisMetricsLog : IMetricsLog
    {
        IMetricsCache metricsCache;

        public RedisMetricsLog(IMetricsCache metricsCache)
        {
            this.metricsCache = metricsCache;
        }

        public void Count(string counterName, long delta)
        {
            if (string.IsNullOrEmpty(counterName) || delta <= 0)
            {
                return;
            }

            metricsCache.IncrementCount(counterName, delta);
        }

        public void Time(string operation, long miliseconds)
        {
            if (string.IsNullOrEmpty(operation) || miliseconds < 0)
            {
                return;
            }

            metricsCache.SetLatency(operation, miliseconds);
        }

        public void Gauge(string gauge, long value)
        {
            if (string.IsNullOrEmpty(gauge) || value < 0)
            {
                return;
            }

            metricsCache.SetGauge(gauge, value);
        }
    }
}
