using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Parsing.Interfaces;
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
            var split = new Split
            {
                name = splitName,
                changeNumber = 121291,
                killed = false,
                seed = 4324324,
                defaultTreatment = "on",
                conditions = new List<ConditionDefinition>(),
                status = "ACTIVE",
                trafficTypeName = "test"
            };

            var splitParser = new Mock<ISplitParser>();
            var redisAdapterMock = new Mock<IRedisAdapter>();
            var splitCache = new RedisSplitCache(redisAdapterMock.Object, splitParser.Object);

            redisAdapterMock
                .Setup(x => x.Get("SPLITIO.split.test_split"))
                .Returns(JsonConvert.SerializeObject(split));

            splitParser
                .Setup(mock => mock.Parse(It.IsAny<Split>()))
                .Returns(new ParsedSplit
                {
                    name = split.name,
                    changeNumber = split.changeNumber,
                    killed = split.killed,
                    seed = split.seed,
                    defaultTreatment = split.defaultTreatment,
                    trafficTypeName = split.trafficTypeName
                });

            //Act
            var result = splitCache.GetSplit(splitName);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(split.name, result.name);
            Assert.AreEqual(split.changeNumber, result.changeNumber);
            Assert.AreEqual(split.killed, result.killed);
            Assert.AreEqual(split.seed, result.seed);
            Assert.AreEqual(split.defaultTreatment, result.defaultTreatment);
            Assert.AreEqual(split.trafficTypeName, result.trafficTypeName);
        }

        [TestMethod]
        public void UseSplitioAndUserPrefix()
        {
            //Arrange
            var splitName = "test_split";
            var split = new Split
            {
                name = splitName,
                changeNumber = 121291,
                killed = false,
                seed = 4324324,
                defaultTreatment = "on",
                conditions = new List<ConditionDefinition>(),
                status = "ACTIVE",
                trafficTypeName = "test"
            };

            var splitParser = new Mock<ISplitParser>();
            var redisAdapterMock = new Mock<IRedisAdapter>();            
            var splitCache = new RedisSplitCache(redisAdapterMock.Object, splitParser.Object, "mycompany");

            redisAdapterMock
                .Setup(x => x.Get("mycompany.SPLITIO.split.test_split"))
                .Returns(JsonConvert.SerializeObject(split));

            splitParser
                .Setup(mock => mock.Parse(It.IsAny<Split>()))
                .Returns(new ParsedSplit
                {
                    name = split.name,
                    changeNumber = split.changeNumber,
                    killed = split.killed,
                    seed = split.seed,
                    defaultTreatment = split.defaultTreatment,
                    trafficTypeName = split.trafficTypeName
                });

            //Act
            var result = splitCache.GetSplit(splitName);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(split.name, result.name);
            Assert.AreEqual(split.changeNumber, result.changeNumber);
            Assert.AreEqual(split.killed, result.killed);
            Assert.AreEqual(split.seed, result.seed);
            Assert.AreEqual(split.defaultTreatment, result.defaultTreatment);
            Assert.AreEqual(split.trafficTypeName, result.trafficTypeName);
        }

        [TestMethod]
        public void UseSplitioAndSdkMachinePrefix()
        {
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            redisAdapterMock.Setup(x => x.IcrBy("SPLITIO/net-1.0.2/10.0.0.1/count.counter_test", 150)).Returns(150);
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2", "machine_name_test");

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
            var cache = new RedisMetricsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2", "machine_name_test", "mycompany");

            //Act
            var result = cache.IncrementCount("counter_test", 150);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.GetCount());
            Assert.AreEqual(150, result.GetDelta());
        }
    }
}
