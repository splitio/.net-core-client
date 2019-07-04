using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Redis.Services.Metrics.Classes;
using Splitio.Services.Cache.Interfaces;

namespace Splitio_net_frameworks_tests.Unit_Tests.Metrics
{
    [TestClass]
    public class RedisMetricsLogUnitTests
    {
        [TestMethod]
        public void CountShouldCallCacheIncrementCount()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);
            
            //Act
            metricsLog.Count("counter_test", 150);

            //Assert
            cacheMock.Verify(mock => mock.IncrementCount("counter_test", 150), Times.Once());
        }

        [TestMethod]
        public void CountShouldNotUpdateMetricIfDeltaIsLessOrEqualThanZero()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);

            //Act
            metricsLog.Count("counter_test", 0);
            metricsLog.Count("counter_test", -1);

            //Assert
            cacheMock.Verify(mock => mock.IncrementCount("counter_test", 0), Times.Never());
            cacheMock.Verify(mock => mock.IncrementCount("counter_test", -1), Times.Never());
        }

        [TestMethod]
        public void CountShouldNotAddMetricIfNoNameSpecified()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);

            //Act
            metricsLog.Count("", 1);

            //Assert
            cacheMock.Verify(mock => mock.IncrementCount("", 1), Times.Never());
        }


        [TestMethod]
        public void TimeShouldCallCacheSetLatency()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);
            
            //Act
            metricsLog.Time("time_test", 1);

            //Assert
            cacheMock.Verify(mock => mock.SetLatency("time_test", 1), Times.Once());
        }

        [TestMethod]
        public void TimeShouldNotUpdateMetricIfDeltaIsLessThanZero()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);

            //Act
            metricsLog.Time("time_test", -1);

            //Assert
            cacheMock.Verify(mock => mock.SetLatency("time_test", -1), Times.Never());
        }

        public void TimeShouldNotAddMetricIfNoNameSpecified()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);

            //Act
            metricsLog.Time("", 1000);

            //Assert
            cacheMock.Verify(mock => mock.SetLatency("", 1000), Times.Never());
        }

        [TestMethod]
        public void GaugeShouldCallCacheSetGauge()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);

            //Act
            metricsLog.Gauge("gauge_test", 1234);

            //Assert
            cacheMock.Verify(mock => mock.SetGauge("gauge_test", 1234), Times.Once());

        }

        [TestMethod]
        public void GaugeShouldNotUpdateMetricIfDeltaIsLessThanZero()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);

            //Act
            metricsLog.Gauge("gauge_test", -1);

            //Assert
            cacheMock.Verify(mock => mock.SetGauge("gauge_test", -1), Times.Never());
        }

        [TestMethod]
        public void GaugeShouldNotAddMetricIfNoNameSpecified()
        {
            //Arrange
            var cacheMock = new Mock<IMetricsCache>();
            var metricsLog = new RedisMetricsLog(cacheMock.Object);

            //Act
            metricsLog.Gauge("", 1000);

            //Assert
            cacheMock.Verify(mock => mock.SetGauge("", 1000), Times.Never());
        }
    }
}
