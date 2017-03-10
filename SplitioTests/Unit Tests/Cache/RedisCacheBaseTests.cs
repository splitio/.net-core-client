using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Cache.Classes;
using Splitio.Domain;
using Newtonsoft.Json;
using Moq;
using Splitio.Services.Cache.Interfaces;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class RedisCacheBaseTests
    {
        [TestMethod]
        public void UseSplitioPrefix()
        {
            //Arrange
            var splitName = "test_split";
            var split = new Split() { name = "test_split", changeNumber = 121291, killed = false, seed = 4324324, defaultTreatment = "on", conditions = new List<ConditionDefinition>(), status = StatusEnum.ACTIVE, trafficTypeName = "test" };
            var splitJson = JsonConvert.SerializeObject(split);
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.Get("SPLITIO.split.test_split")).Returns(splitJson);
            var splitCache = new RedisSplitCache(redisAdapterMock.Object);

            //Act
            var result = splitCache.GetSplit(splitName);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UseSplitioAndUserPrefix()
        {
            //Arrange
            var splitName = "test_split";
            var split = new Split() { name = "test_split", changeNumber = 121291, killed = false, seed = 4324324, defaultTreatment = "on", conditions = new List<ConditionDefinition>(), status = StatusEnum.ACTIVE, trafficTypeName = "test" };
            var splitJson = JsonConvert.SerializeObject(split);
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.Get("mycompany.SPLITIO.split.test_split")).Returns(splitJson);
            var splitCache = new RedisSplitCache(redisAdapterMock.Object, "mycompany");

            //Act
            var result = splitCache.GetSplit(splitName);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UseSplitioAndSdkMachinePrefix()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.IcrBy("SPLITIO/net-1.0.2/10.0.0.1/count.counter_test", 150)).Returns(150);
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

            //Act
            var result = cache.IncrementCount("counter_test", 150);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.GetCount());
            Assert.AreEqual(150, result.GetDelta());
        }

        [TestMethod]
        public void UseSplitioAndSdkMachineAndUserPrefix()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.IcrBy("mycompany.SPLITIO/net-1.0.2/10.0.0.1/count.counter_test", 150)).Returns(150);
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2", "mycompany");

            //Act
            var result = cache.IncrementCount("counter_test", 150);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.GetCount());
            Assert.AreEqual(150, result.GetDelta());
        }
    }
}
