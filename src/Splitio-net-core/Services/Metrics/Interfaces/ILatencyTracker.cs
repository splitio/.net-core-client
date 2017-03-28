namespace Splitio.Services.Metrics.Interfaces
{
    public interface ILatencyTracker
    {

        void AddLatencyMillis(long millis);

        void AddLatencyMicros(long micros);

        long[] GetLatencies();

        long GetLatency(int index);

        long GetBucketForLatencyMillis(long latency);

        long GetBucketForLatencyMicros(long latency);

        void SetLatencyCount(int index, long count);

    }
}
