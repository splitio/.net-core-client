using Splitio.Services.Metrics.Classes;
using Splitio.Services.Metrics.Interfaces;
using System.Collections.Generic;

namespace Splitio.Services.Cache.Interfaces
{
    public interface IMetricsCache
    {
        Counter GetCount(string name);

        Counter IncrementCount(string name, long delta);

        Dictionary<string, Counter> FetchAllCountersAndClear();

        void SetGauge(string name, long gauge);

        long GetGauge(string name);

        Dictionary<string, long> FetchAllGaugesAndClear();

        ILatencyTracker GetLatencyTracker(string name);

        void SetLatency(string name, long value);

        Dictionary<string, ILatencyTracker> FetchAllLatencyTrackersAndClear();
    }
}
