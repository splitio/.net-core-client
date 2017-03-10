using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using Splitio.Services.Metrics.Classes;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Metrics.Interfaces;
using System.Linq;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class InMemoryMetricsCacheTests
    {
        [TestMethod]
        public void IncrementCountShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var cache = new InMemoryMetricsCache(counters);

            //Act
            cache.IncrementCount("counter_test", 150);

            //Assert
            Counter counter = cache.GetCount("counter_test");
            Assert.IsNotNull(counter);
            Assert.AreEqual(1, counter.GetCount());
            Assert.AreEqual(150, counter.GetDelta());
        }

        [TestMethod]
        public void IncrementCountShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var cache = new InMemoryMetricsCache(counters);

            //Act
            cache.IncrementCount("counter_test", 150);
            cache.IncrementCount("counter_test", 10);

            //Assert
            Counter counter = cache.GetCount("counter_test");
            Assert.AreEqual(2, counter.GetCount());
            Assert.AreEqual(160, counter.GetDelta());

        }


        [TestMethod]
        public void SetLatencyShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var cache = new InMemoryMetricsCache(null, timers);

            //Act
            cache.SetLatency("time_test", 1);

            //Assert
            ILatencyTracker timer = cache.GetLatencyTracker("time_test");
            Assert.IsNotNull(timer);
            Assert.AreEqual(1, timer.GetLatency(0));
            long[] latencies = timer.GetLatencies();
            Assert.AreEqual(1, latencies.Sum());
        }

        [TestMethod]
        public void SetLatencyShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var cache = new InMemoryMetricsCache(null, timers);

            //Act
            cache.SetLatency("time_test", 1);
            cache.SetLatency("time_test", 9);
            cache.SetLatency("time_test", 8);

            //Assert
            ILatencyTracker timer = cache.GetLatencyTracker("time_test");
            Assert.AreEqual(1, timer.GetLatency(0));
            Assert.AreEqual(2, timer.GetLatency(6));
            long[] latencies = timer.GetLatencies();
            Assert.AreEqual(3, latencies.Sum());
        }


        [TestMethod]
        public void SetGaugeShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var cache = new InMemoryMetricsCache(null, null, gauges);

            //Act
            cache.SetGauge("gauge_test", 1234);

            //Assert
            long gauge = cache.GetGauge("gauge_test");
            Assert.IsNotNull(gauge);
            Assert.AreEqual(1234, gauge);
        }

        [TestMethod]
        public void SetGaugeShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var cache = new InMemoryMetricsCache(null, null, gauges);

            //Act
            cache.SetGauge("gauge_test", 1234);
            cache.SetGauge("gauge_test", 4567);

            //Assert
            long gauge = cache.GetGauge("gauge_test");
            Assert.AreEqual(4567, gauge);
        }

        [TestMethod]
        public void FetchAllCountMetricsSuccessfully()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var cache = new InMemoryMetricsCache(counters);
            cache.IncrementCount("counter_test", 150);
            cache.IncrementCount("counter_test_2", 123);
            cache.IncrementCount("counter_test_2", 1);

            //Act
            var result = cache.FetchAllCountersAndClear();

            //Assert
            Assert.IsNotNull(result);
            var result1 = result.First(x => x.Key == "counter_test");
            Assert.IsNotNull(result1);
            Assert.AreEqual(150, result1.Value.GetDelta());
            var result2 = result.First(x => x.Key == "counter_test_2");
            Assert.IsNotNull(result2);
            Assert.AreEqual(124, result2.Value.GetDelta());
            Assert.AreEqual(0, counters.Count());
        }

        [TestMethod]
        public void FetchAllGaugeMetricsSuccessfully()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var cache = new InMemoryMetricsCache(null, null, gauges);
            cache.SetGauge("gauge_test", 1234);
            cache.SetGauge("gauge_test_1", 4567);

            //Act
            var result = cache.FetchAllGaugesAndClear();

            //Assert
            Assert.IsNotNull(result);
            var result1 = result.First(x => x.Key == "gauge_test");
            Assert.IsNotNull(result1);
            Assert.AreEqual(1234, result1.Value);
            var result2 = result.First(x => x.Key == "gauge_test_1");
            Assert.IsNotNull(result2);
            Assert.AreEqual(4567, result2.Value);
            Assert.AreEqual(0, gauges.Count());
        }


        [TestMethod]
        public void FetchAllLatencyMetricsSuccessfully()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var cache = new InMemoryMetricsCache(null, timers);
            cache.SetLatency("time_test", 1);
            cache.SetLatency("time_test", 9);
            cache.SetLatency("time_test", 8);
            cache.SetLatency("time_test_2", 8);

            //Act
            var result = cache.FetchAllLatencyTrackersAndClear();

            //Assert
            Assert.IsNotNull(result);
            var result1 = result.First(x => x.Key == "time_test");
            Assert.IsNotNull(result1);
            Assert.AreEqual(1, result1.Value.GetLatency(0));
            Assert.AreEqual(2, result1.Value.GetLatency(6));
            long[] latencies = result1.Value.GetLatencies();
            Assert.AreEqual(3, latencies.Sum());
            var result2 = result.First(x => x.Key == "time_test_2");
            Assert.IsNotNull(result2);
            Assert.AreEqual(1, result2.Value.GetLatency(6));
            Assert.AreEqual(0, timers.Count());
        }
    }
}
