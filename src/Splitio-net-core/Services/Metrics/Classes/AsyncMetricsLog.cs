using NLog;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Metrics.Interfaces;
using System;
using System.Threading.Tasks;

namespace Splitio.Services.Metrics.Classes
{
    public class AsyncMetricsLog : IMetricsLog
    {
        IMetricsLog worker;

        protected static readonly Logger Logger = LogManager.GetLogger(typeof(AsyncMetricsLog).ToString());

        public AsyncMetricsLog(IMetricsSdkApiClient apiClient, IMetricsCache metricsCache, int maxCountCalls = -1, int maxTimeBetweenCalls = -1)
        {
            worker = new InMemoryMetricsLog(apiClient, metricsCache, maxCountCalls, maxTimeBetweenCalls);
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
                Logger.Error(e, "Exception running count metrics task");
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
                Logger.Error(e, "Exception running time metrics task");
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
                Logger.Error(e, "Exception running gauge metrics task");
            }
        }
    }
}
