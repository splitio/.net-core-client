using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Metrics.Classes;
using System.Collections.Concurrent;
using Splitio.Services.Metrics.Interfaces;
using System.Linq;
using Moq;
using Splitio.Services.Cache.Classes;

namespace Splitio_Tests.Unit_Tests.Metrics
{
    [TestClass]
    public class InMemoryMetricsLogUnitTests
    {
        [TestMethod]
        public void CountShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var cache = new InMemoryMetricsCache(counters);
            var metricsLog = new InMemoryMetricsLog(null, cache);
            
            //Act
            metricsLog.Count("counter_test", 150);

            //Assert
            Counter counter = cache.GetCount("counter_test");
            Assert.IsNotNull(counter);
            Assert.AreEqual(1, counter.GetCount());
            Assert.AreEqual(150, counter.GetDelta());
        }

        [TestMethod]
        public void CountShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var cache = new InMemoryMetricsCache(counters);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Count("counter_test", 150);
            metricsLog.Count("counter_test", 10);

            //Assert
            Counter counter = cache.GetCount("counter_test");
            Assert.AreEqual(2, counter.GetCount());
            Assert.AreEqual(160, counter.GetDelta());

        }


        [TestMethod]
        public void CountShouldNotUpdateMetricIfDeltaIsLessOrEqualThanZero()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var cache = new InMemoryMetricsCache(counters);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Count("counter_test", 0);
            metricsLog.Count("counter_test", -1);

            //Assert
            Counter counter = cache.GetCount("counter_test");
            Assert.IsNull(counter);
        }

        [TestMethod]
        public void CountShouldNotAddMetricIfNoNameSpecified()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var cache = new InMemoryMetricsCache(counters);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Count("", 1);

            //Assert
            Counter counter = cache.GetCount("");
            Assert.IsNull(counter);
        }


        [TestMethod]
        public void TimeShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var cache = new InMemoryMetricsCache(null, timers);
            var metricsLog = new InMemoryMetricsLog(null, cache);
            
            //Act
            metricsLog.Time("time_test", 1);

            //Assert
            ILatencyTracker timer = cache.GetLatencyTracker("time_test");
            Assert.IsNotNull(timer);
            Assert.AreEqual(1, timer.GetLatency(0));
            long[] latencies = timer.GetLatencies();
            Assert.AreEqual(1, latencies.Sum());
        }

        [TestMethod]
        public void TimeShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var cache = new InMemoryMetricsCache(null, timers);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Time("time_test", 1);
            metricsLog.Time("time_test", 9);
            metricsLog.Time("time_test", 8);
          
            //Assert
            ILatencyTracker timer = cache.GetLatencyTracker("time_test");
            Assert.AreEqual(1, timer.GetLatency(0));
            Assert.AreEqual(2, timer.GetLatency(6));
            long[] latencies = timer.GetLatencies();
            Assert.AreEqual(3, latencies.Sum());
        }


        [TestMethod]
        public void TimeShouldNotUpdateMetricIfDeltaIsLessThanZero()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var cache = new InMemoryMetricsCache(null, timers);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Time("time_test", -1);

            //Assert
            ILatencyTracker timer = cache.GetLatencyTracker("time_test");
            Assert.IsNull(timer);
        }

        public void TimeShouldNotAddMetricIfNoNameSpecified()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var cache = new InMemoryMetricsCache(null, timers);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Time("", 1000);

            //Assert
            ILatencyTracker timer = cache.GetLatencyTracker("");
            Assert.IsNull(timer);
        }

        [TestMethod]
        public void GaugeShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var cache = new InMemoryMetricsCache(null, null, gauges);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Gauge("gauge_test", 1234);

            //Assert
            long gauge = cache.GetGauge("gauge_test");
            Assert.IsNotNull(gauge);
            Assert.AreEqual(1234, gauge);
        }

        [TestMethod]
        public void GaugeShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var cache = new InMemoryMetricsCache(null, null, gauges);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Gauge("gauge_test", 1234);
            metricsLog.Gauge("gauge_test", 4567);

            //Assert
            long gauge = cache.GetGauge("gauge_test");
            Assert.AreEqual(4567, gauge);
        }

        [TestMethod]
        public void GaugeShouldNotUpdateMetricIfDeltaIsLessThanZero()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var cache = new InMemoryMetricsCache(null, null, gauges);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Gauge("gauge_test", -1);

            //Assert
            long gauge = cache.GetGauge("gauge_test");
            Assert.AreEqual(0, gauge);
        }

        [TestMethod]
        public void GaugeShouldNotAddMetricIfNoNameSpecified()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var cache = new InMemoryMetricsCache(null, null, gauges);
            var metricsLog = new InMemoryMetricsLog(null, cache);

            //Act
            metricsLog.Gauge("", 1000);

            //Assert
            long gauge = cache.GetGauge("");
            Assert.AreEqual(0, gauge);
        }


        [TestMethod]
        public void SendCountMetricsSuccessfully()
        {
            //Arrange
            var counters = new ConcurrentDictionary<string, Counter>();
            var metricsApiClientMock = new Mock<IMetricsSdkApiClient>();
            var cache = new InMemoryMetricsCache(counters);
            var metricsLog = new InMemoryMetricsLog(metricsApiClientMock.Object, cache, 1);
            
            //Act
            metricsLog.Count("counter_test", 150);

            //Assert
            metricsApiClientMock.Verify(x => x.SendCountMetrics(It.IsAny<string>()));
        }

        [TestMethod]
        public void SendTimeMetricsSuccessfully()
        {
            //Arrange
            var timers = new ConcurrentDictionary<string, ILatencyTracker>();
            var metricsApiClientMock = new Mock<IMetricsSdkApiClient>();
            var cache = new InMemoryMetricsCache(null, timers);
            var metricsLog = new InMemoryMetricsLog(metricsApiClientMock.Object, cache, 1, -1);

            //Act
            metricsLog.Time("time_test", 1);

            //Assert
            metricsApiClientMock.Verify(x => x.SendTimeMetrics(It.IsAny<string>()));
        }

        [TestMethod]
        public void SendGaugeMetricsSuccessfully()
        {
            //Arrange
            var gauges = new ConcurrentDictionary<string, long>();
            var metricsApiClientMock = new Mock<IMetricsSdkApiClient>();
            var cache = new InMemoryMetricsCache(null, null, gauges);
            var metricsLog = new InMemoryMetricsLog(metricsApiClientMock.Object, cache, 1);

            //Act
            metricsLog.Gauge("gauge_test", 1234);

            //Assert
            metricsApiClientMock.Verify(x => x.SendGaugeMetrics(It.IsAny<string>()));
        }

    }
}
