namespace Splitio.Services.Metrics.Interfaces
{
    public interface IMetricsLog
    {
        void Count(string counterName, long delta);

        void Time(string operation, long miliseconds);

        void Gauge(string gauge, long value);

        void Clear();
    }
}
