using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Metrics.Classes;
using Splitio.Services.Metrics.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Splitio.Services.Cache.Classes
{
    public class InMemoryMetricsCache : IMetricsCache
    {
        private ConcurrentDictionary<string, Counter> countMetrics;
        private ConcurrentDictionary<string, ILatencyTracker> timeMetrics;
        private ConcurrentDictionary<string, long> gaugeMetrics;

        public InMemoryMetricsCache(ConcurrentDictionary<string, Counter> countMetrics = null, ConcurrentDictionary<string, ILatencyTracker> timeMetrics = null, ConcurrentDictionary<string, long> gaugeMetrics = null)
        {
            this.countMetrics = countMetrics ?? new ConcurrentDictionary<string, Counter>();
            this.timeMetrics = timeMetrics ?? new ConcurrentDictionary<string, ILatencyTracker>();
            this.gaugeMetrics = gaugeMetrics ?? new ConcurrentDictionary<string, long>();
        }

        public Counter IncrementCount(string name, long delta)
        {
            Counter counter;

            countMetrics.TryGetValue(name, out counter);

            if (counter == null)
            {
                counter = new Counter();
            }

            counter.AddDelta(delta);
            countMetrics.TryAdd(name, counter); 

            return counter;
        }

        public Counter GetCount(string name)
        {
            Counter counter;

            countMetrics.TryGetValue(name, out counter);
            return counter; 
        }

        public void SetGauge(string name, long value)
        {
            long gauge;

            var result = gaugeMetrics.TryGetValue(name, out gauge);

            if (!result)
            {
                gaugeMetrics.TryAdd(name, value);
            }
            else
            {
                gaugeMetrics[name] = value;
            }
        }

        public long GetGauge(string name)
        {
            long value;
            var hasResult = gaugeMetrics.TryGetValue(name, out value);
            return hasResult ? value : 0;
        }

        public void SetLatency(string name, long value)
        {
            ILatencyTracker tracker;
            timeMetrics.TryGetValue(name, out tracker);

            if (tracker == null)
            {
                tracker = new BinarySearchLatencyTracker();
            }

            tracker.AddLatencyMillis(value);
            timeMetrics.TryAdd(name, tracker);
        }

        public ILatencyTracker GetLatencyTracker(string name)
        {
            ILatencyTracker tracker;
            timeMetrics.TryGetValue(name, out tracker);
            return tracker;
        }

        public Dictionary<string, Counter> FetchAllCountersAndClear()
        {
            var result = new Dictionary<string, Counter>(countMetrics);
            countMetrics.Clear();
            return result;
        }

        public Dictionary<string, long> FetchAllGaugesAndClear()
        {
            var result = new Dictionary<string, long>(gaugeMetrics);
            gaugeMetrics.Clear();
            return result;
        }

        public Dictionary<string, ILatencyTracker> FetchAllLatencyTrackersAndClear()
        {
            var result = new Dictionary<string, ILatencyTracker>(timeMetrics);
            timeMetrics.Clear();
            return result;
        }
    }
}
