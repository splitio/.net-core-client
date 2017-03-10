using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Metrics.Interfaces;
using System.Linq;
using StackExchange.Redis;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class RedisMetricsCacheTests
    {
        private const string metricsLatencyKeyPrefix = "SPLITIO/net-1.0.2/10.0.0.1/latency.{metricName}.bucket.{bucketNumber}";
        private const string metricsCountKeyPrefix = "SPLITIO/net-1.0.2/10.0.0.1/count.";
        private const string metricsGaugeKeyPrefix = "SPLITIO/net-1.0.2/10.0.0.1/gauge.";

        [TestMethod]
        public void IncrementCountShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.IcrBy(metricsCountKeyPrefix + "counter_test", 150)).Returns(150);
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

            //Act
            var result = cache.IncrementCount("counter_test", 150);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.GetCount());
            Assert.AreEqual(150, result.GetDelta());
        }

        [TestMethod]
        public void IncrementCountShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.IcrBy(metricsCountKeyPrefix + "counter_test", 150)).Returns(150);
            redisAdapterMock.Setup(x => x.IcrBy(metricsCountKeyPrefix + "counter_test", 10)).Returns(160);

            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

            //Act
            cache.IncrementCount("counter_test", 150);
            var result = cache.IncrementCount("counter_test", 10);

            //Assert
            Assert.AreEqual(160, result.GetDelta());
        }


        [TestMethod]
        public void SetLatencyShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            var bucketsPattern = metricsLatencyKeyPrefix.Replace("{metricName}", "time_test").Replace("{bucketNumber}", "*");
            var currentBucketPattern = bucketsPattern.Replace("*", "0");
            redisAdapterMock.Setup(x => x.Keys(bucketsPattern)).Returns(new RedisKey[]{ currentBucketPattern });
            redisAdapterMock.Setup(x => x.Get(currentBucketPattern)).Returns("1");
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

            //Act
            cache.SetLatency("time_test", 1);

            //Assert
            ILatencyTracker timer = cache.GetLatencyTracker("time_test");
            Assert.IsNotNull(timer);
            Assert.AreEqual(1, timer.GetLatency(0));
            long[] latencies = timer.GetLatencies();
            Assert.AreEqual(1, latencies.Sum());
            redisAdapterMock.Verify(mock => mock.IcrBy(metricsLatencyKeyPrefix.Replace("{metricName}", "time_test").Replace("{bucketNumber}", "0"), 1));
        }

        
        [TestMethod]
        public void SetLatencyShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            var bucketsPattern = metricsLatencyKeyPrefix.Replace("{metricName}", "time_test").Replace("{bucketNumber}", "*");
            var currentBucketPattern = bucketsPattern.Replace("*", "0");
            var currentBucketPattern2 = bucketsPattern.Replace("*", "6");
            redisAdapterMock.Setup(x => x.Keys(bucketsPattern)).Returns(new RedisKey[] { currentBucketPattern, currentBucketPattern2 });
            redisAdapterMock.Setup(x => x.Get(currentBucketPattern)).Returns("1");
            redisAdapterMock.Setup(x => x.Get(currentBucketPattern2)).Returns("2");
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

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
            redisAdapterMock.Verify(mock => mock.IcrBy(metricsLatencyKeyPrefix.Replace("{metricName}", "time_test").Replace("{bucketNumber}", "0"), 1), Times.Once());
            redisAdapterMock.Verify(mock => mock.IcrBy(metricsLatencyKeyPrefix.Replace("{metricName}", "time_test").Replace("{bucketNumber}", "6"), 1), Times.Exactly(2));
        }

        
        [TestMethod]
        public void SetGaugeShouldAddNewMetricWhenNotExisting()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.Set(metricsGaugeKeyPrefix + "gauge_test", "150"));
            redisAdapterMock.Setup(x => x.Get(metricsGaugeKeyPrefix + "gauge_test")).Returns("150");
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

            //Act
            cache.SetGauge("gauge_test", 150);

            //Assert
            long gauge = cache.GetGauge("gauge_test");
            Assert.IsNotNull(gauge);
            Assert.AreEqual(150, gauge);
        }

        [TestMethod]
        public void SetGaugeShouldUpdateMetricWhenExisting()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.Set(metricsGaugeKeyPrefix + "gauge_test", "1234"));
            redisAdapterMock.Setup(x => x.Set(metricsGaugeKeyPrefix + "gauge_test", "4567"));
            redisAdapterMock.Setup(x => x.Get(metricsGaugeKeyPrefix + "gauge_test")).Returns("4567");
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

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
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.IcrBy(metricsCountKeyPrefix + "counter_test", 150)).Returns(150);
            redisAdapterMock.Setup(x => x.IcrBy(metricsCountKeyPrefix + "counter_test_2", 123)).Returns(123);
            redisAdapterMock.Setup(x => x.IcrBy(metricsCountKeyPrefix + "counter_test_2", 1)).Returns(124);
            redisAdapterMock.Setup(x => x.Keys(metricsCountKeyPrefix + "*")).Returns(new RedisKey[] { metricsCountKeyPrefix + "counter_test", metricsCountKeyPrefix + "counter_test_2" });
            redisAdapterMock.Setup(x => x.Get(metricsCountKeyPrefix + "counter_test")).Returns("150");
            redisAdapterMock.Setup(x => x.Get(metricsCountKeyPrefix + "counter_test_2")).Returns("124");

            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");
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
        }
        
        [TestMethod]
        public void FetchAllGaugeMetricsSuccessfully()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.IcrBy(metricsGaugeKeyPrefix + "gauge_test", 1234));
            redisAdapterMock.Setup(x => x.IcrBy(metricsGaugeKeyPrefix + "gauge_test_1", 4567));
            redisAdapterMock.Setup(x => x.Keys(metricsGaugeKeyPrefix + "*")).Returns(new RedisKey[] { metricsGaugeKeyPrefix + "gauge_test", metricsGaugeKeyPrefix + "gauge_test_1" });
            redisAdapterMock.Setup(x => x.Get(metricsGaugeKeyPrefix + "gauge_test")).Returns("1234");
            redisAdapterMock.Setup(x => x.Get(metricsGaugeKeyPrefix + "gauge_test_1")).Returns("4567");

            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");
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
        }
        

        [TestMethod] 
        public void FetchAllLatencyMetricsSuccessfully()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            var bucketsPattern =  metricsLatencyKeyPrefix.Replace("{metricName}.bucket.{bucketNumber}", "*");
            var pattern = metricsLatencyKeyPrefix.Replace("{metricName}", "time_test").Replace("{bucketNumber}", "*");
            var currentBucketPattern = pattern.Replace("*", "0");
            var currentBucketPattern2 = pattern.Replace("*", "6");
            var pattern2 = metricsLatencyKeyPrefix.Replace("{metricName}", "time_test_2").Replace("{bucketNumber}", "*");
            var currentBucketPattern3 = pattern2.Replace("*", "6");
            redisAdapterMock.Setup(x => x.Keys(bucketsPattern)).Returns(new RedisKey[] { currentBucketPattern, currentBucketPattern2, currentBucketPattern3 });
            redisAdapterMock.Setup(x => x.Get(currentBucketPattern)).Returns("1");
            redisAdapterMock.Setup(x => x.Get(currentBucketPattern2)).Returns("2");
            redisAdapterMock.Setup(x => x.Get(currentBucketPattern3)).Returns("1");


            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");
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
        }
    }
}
