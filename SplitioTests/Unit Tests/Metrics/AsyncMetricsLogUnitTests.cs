using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Metrics.Classes;
using System.Collections.Concurrent;
using System.Threading;
using Splitio.Services.Metrics.Interfaces;
using System.Linq;
using Splitio.Services.Cache.Classes;

namespace Splitio_Tests.Unit_Tests.Metrics
{
    [TestClass]
    public class AsyncMetricsLogUnitTests
    {
        [TestMethod]
        public void CountSucessfully()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var metricsCache = new InMemoryMetricsCache(counters);
            var metricsLog = new AsyncMetricsLog(null, metricsCache, 10, 3000);

            //Act
            metricsLog.Count("counter_test", 150);

            //Assert
            Thread.Sleep(2000);
            Counter counter = metricsCache.GetCount("counter_test");
            Assert.IsNotNull(counter);
            Assert.AreEqual(1, counter.GetCount());
            Assert.AreEqual(150, counter.GetDelta());
        }


        [TestMethod]
        public void TimeSucessfully()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var metricsCache = new InMemoryMetricsCache(null, timers);
            var metricsLog = new AsyncMetricsLog(null, metricsCache, 10, 3000);

            //Act
            metricsLog.Time("time_test", 1);

            //Assert
            Thread.Sleep(2000);
            ILatencyTracker timer = metricsCache.GetLatencyTracker("time_test");
            Assert.IsNotNull(timer);
            Assert.AreEqual(1, timer.GetLatency(0));
            long[] latencies = timer.GetLatencies();
            Assert.AreEqual(1, latencies.Sum());
        }


        [TestMethod]
        public void GaugeSucessfully()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var metricsCache = new InMemoryMetricsCache(null, null, gauges);
            var metricsLog = new AsyncMetricsLog(null, metricsCache, 10, 3000);

            //Act
            metricsLog.Gauge("gauge_test", 1234);

            //Assert
            Thread.Sleep(2000);
            long gauge = metricsCache.GetGauge("gauge_test");
            Assert.IsNotNull(gauge);
            Assert.AreEqual(1234, gauge);
        }
    }
}
