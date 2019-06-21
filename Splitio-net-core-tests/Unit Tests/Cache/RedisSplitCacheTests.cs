using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class RedisSplitCacheTests
    {
        private const string splitKeyPrefix = "SPLITIO.split.";
        private const string splitsKeyPrefix = "SPLITIO.splits.";
        private const string trafficTypeKeyPrefix = "SPLITIO.trafficType.";

        private readonly Mock<IRedisAdapter> _redisAdapterMock;
        private readonly RedisSplitCache _redisSplitCache;

        public RedisSplitCacheTests()
        {
            _redisAdapterMock = new Mock<IRedisAdapter>();

            _redisSplitCache = new RedisSplitCache(_redisAdapterMock.Object);
        }

        [TestMethod]
        public void AddAndGetSplitTest()
        {
            //Arrange
            var splitName = "test_split";
            var split = BuildSplit(splitName);
            var splitJson = JsonConvert.SerializeObject(split);

            _redisAdapterMock
                .Setup(x => x.Set(splitKeyPrefix + "test_split", splitJson))
                .Returns(true);

            _redisAdapterMock
                .Setup(x => x.Get(splitKeyPrefix + "test_split"))
                .Returns(splitJson);

            //Act
            _redisSplitCache.AddSplit(splitName, split);

            //Assert
            var result = (Split)_redisSplitCache.GetSplit(splitName);
            Assert.IsNotNull(result);
            Assert.AreEqual(splitName, result.name);
        }

        [TestMethod]
        public void AddDuplicateSplitTest()
        {
            //Arrange
            var splitName = "test_split";
            var split = BuildSplit(splitName);
            var splitJson = JsonConvert.SerializeObject(split);

            _redisAdapterMock
                .Setup(x => x.Keys(splitKeyPrefix + "*"))
                .Returns(new RedisKey[]{"test_split"});

            _redisAdapterMock
                .Setup(x => x.Get(It.IsAny<RedisKey[]>()))
                .Returns(new RedisValue[]{splitJson});

            var split1 = new Split { name = splitName };
            _redisSplitCache.AddSplit(splitName, split1);

            var split2 = new Split { name = "test_split_2" };

            //Act
            _redisSplitCache.AddSplit(splitName, split2);

            //Assert
            var result = _redisSplitCache.GetAllSplits();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(((Split)result[0]).name, split1.name);
            Assert.AreNotEqual(((Split)result[0]).name, split2.name);
        }

        [TestMethod]
        public void GetInexistentSplitOrRedisExceptionShouldReturnNull()
        {
            //Arrange
            var splitName = "test_split";
            string value = null;

            _redisAdapterMock
                .Setup(x => x.Get(splitKeyPrefix + "test_split"))
                .Returns(value);

            //Act
            var result = _redisSplitCache.GetSplit(splitName);

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void RemoveSplitTest()
        {
            //Arrange
            var splitName = "test_split";
            string value = null;

            _redisAdapterMock
                .Setup(x => x.Del(splitKeyPrefix + "test_split"))
                .Returns(true);

            _redisAdapterMock
                .Setup(x => x.Get(splitKeyPrefix + "test_split"))
                .Returns(value);

            //Act
            var isRemoved = _redisSplitCache.RemoveSplit(splitName);
            var result = _redisSplitCache.GetSplit(splitName);

            //Assert
            Assert.IsTrue(isRemoved);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void RemoveSplitShouldReturnFalseOnException()
        {
            //Arrange
            var splitName = "test_split";

            _redisAdapterMock
                .Setup(x => x.Del(splitKeyPrefix + "test_split"))
                .Returns(false);

            //Act
            var isRemoved = _redisSplitCache.RemoveSplit(splitName);

            //Assert
            Assert.IsFalse(isRemoved);
        }

        [TestMethod]
        public void RemoveSplitsTest()
        {
            //Arrange
            var splitName = "test_split";
            string value = null;

            _redisAdapterMock
                .Setup(x => x.Del(It.IsAny<RedisKey[]>()))
                .Returns(1);

            _redisAdapterMock
                .Setup(x => x.Get(splitKeyPrefix + "test_split"))
                .Returns(value);

            //Act
            var removedCount = _redisSplitCache.RemoveSplits(new List<string>(){splitName});
            var result = _redisSplitCache.GetSplit(splitName);

            //Assert
            Assert.AreEqual(1, removedCount);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SetAndGetChangeNumberTest()
        {
            //Arrange
            var changeNumber = 1234;

            _redisAdapterMock
                .Setup(x => x.Set(splitsKeyPrefix + "till", changeNumber.ToString()))
                .Returns(true);

            _redisAdapterMock
                .Setup(x => x.Get(splitsKeyPrefix + "till"))
                .Returns(changeNumber.ToString());

            //Act
            _redisSplitCache.SetChangeNumber(changeNumber);
            var result = _redisSplitCache.GetChangeNumber();

            //Assert
            Assert.AreEqual(changeNumber, result);
        }

        [TestMethod]
        public void GetChangeNumberWhenNotSetOrRedisThrowsException()
        {
            //Arrange
            var changeNumber = -1;

            _redisAdapterMock
                .Setup(x => x.Get(splitsKeyPrefix + "till"))
                .Returns(string.Empty);

            //Act
            var result = _redisSplitCache.GetChangeNumber();

            //Assert
            Assert.AreEqual(changeNumber, result);
        }

        [TestMethod]
        public void GetAllSplitsSuccessfully()
        {
            //Arrange
            var splitName = "test_split";
            var splitName2 = "test_split2";
            var split = BuildSplit(splitName);
            var split2 = BuildSplit(splitName2);
            var splitJson = JsonConvert.SerializeObject(split);
            var splitJson2 = JsonConvert.SerializeObject(split);

            _redisAdapterMock
                .Setup(x => x.Set(It.IsAny<string>(), splitJson))
                .Returns(true);

            _redisAdapterMock
                .Setup(x => x.Set(It.IsAny<string>(), splitJson2))
                .Returns(true);

            _redisAdapterMock
                .Setup(x => x.Keys(splitKeyPrefix + "*"))
                .Returns(new RedisKey[] { "test_split", "test_split2" });

            _redisAdapterMock
                .Setup(x => x.Get(It.IsAny<RedisKey[]>()))
                .Returns(new RedisValue[] { splitJson, splitJson2 });

            _redisSplitCache.AddSplit(splitName, new Split { name = splitName });
            _redisSplitCache.AddSplit(splitName2, new Split { name = splitName2 });

            //Act
            var result = _redisSplitCache.GetAllSplits();

            //Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetAllSplitsShouldReturnEmptyListIfGetReturnsEmpty()
        {
            //Arrange
            _redisAdapterMock
                .Setup(x => x.Keys(splitKeyPrefix + "*"))
                .Returns(new RedisKey[] { });

            _redisAdapterMock
                .Setup(x => x.Get(It.IsAny<RedisKey[]>()))
                .Returns(new RedisValue[] { });

            //Act
            var result = _redisSplitCache.GetAllSplits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetAllSplitsShouldReturnEmptyListIfGetReturnsNull()
        {
            //Arrange
            RedisValue[] expectedResult = null;

            _redisAdapterMock
                .Setup(x => x.Keys(splitKeyPrefix + "*"))
                .Returns(new RedisKey[] { });

            _redisAdapterMock
                .Setup(x => x.Get(It.IsAny<RedisKey[]>()))
                .Returns(expectedResult);

            //Act
            var result = _redisSplitCache.GetAllSplits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FlushTest()
        {
            //Act
            _redisSplitCache.Flush();

            //Assert
            _redisAdapterMock.Verify(mock => mock.Flush(), Times.Once());
        }

        [TestMethod]
        public void GetKeysTestSuccessfully()
        {
            //Arrange
            _redisAdapterMock
                .Setup(x => x.Keys(splitKeyPrefix + "*"))
                .Returns(new RedisKey[] { "test_split", "test_split2" });

            //Act
            var result = _redisSplitCache.GetKeys();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetKeysShouldReturnEmptyResultIfNoKeysOrRedisException()
        {
            //Arrange
            _redisAdapterMock
                .Setup(x => x.Keys(splitKeyPrefix + "*"))
                .Returns(new RedisKey[] { });

            //Act
            var result = _redisSplitCache.GetKeys();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void TrafficTypeExists_WhenHasQuantity_ReturnsTrue()
        {
            //Arrange
            var trafficType = "test";

            var ttKey = $"{trafficTypeKeyPrefix}{trafficType}";

            _redisAdapterMock
                .Setup(mock => mock.Get(ttKey))
                .Returns("1");

            //Act
            var result = _redisSplitCache.TrafficTypeExists(trafficType);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TrafficTypeExists_WhenQuantityIs0_ReturnsFalse()
        {
            //Arrange
            var trafficType = "test";

            var ttKey = $"{trafficTypeKeyPrefix}{trafficType}";

            _redisAdapterMock
                .Setup(mock => mock.Get(ttKey))
                .Returns("0");

            //Act
            var result = _redisSplitCache.TrafficTypeExists(trafficType);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TrafficTypeExists_WhenKeyDoesNotExist_ReturnsFalse()
        {
            //Arrange
            var trafficType = "test";

            var ttKey = $"{trafficTypeKeyPrefix}{trafficType}";

            _redisAdapterMock
                .Setup(mock => mock.Get(ttKey))
                .Returns((string)null);

            //Act
            var result = _redisSplitCache.TrafficTypeExists(trafficType);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TrafficTypeExists_WhenValueIsEmpty_ReturnsFalse()
        {
            //Arrange
            var trafficType = "test";

            var ttKey = $"{trafficTypeKeyPrefix}{trafficType}";

            _redisAdapterMock
                .Setup(mock => mock.Get(ttKey))
                .Returns(string.Empty);

            //Act
            var result = _redisSplitCache.TrafficTypeExists(trafficType);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TrafficTypeExists_WhenKeyIsNull_ReturnsFalse()
        {
            //Arrange
            var trafficType = "test";

            var ttKey = $"{trafficTypeKeyPrefix}{trafficType}";

            _redisAdapterMock
                .Setup(mock => mock.Get(ttKey))
                .Returns(string.Empty);

            //Act
            var result = _redisSplitCache.TrafficTypeExists(null);

            //Assert
            Assert.IsFalse(result);
        }

        // #############################################################################################################33
        [TestMethod]
        public void AddSplit_SaveTrafficType()
        {
            //Arrange
            var splitName = "test_split";
            var split = BuildSplit(splitName);
            var splitJson = JsonConvert.SerializeObject(split);

            _redisAdapterMock
                .Setup(x => x.Set(splitKeyPrefix + "test_split", splitJson))
                .Returns(true);

            var ttKey = $"{trafficTypeKeyPrefix}{split.trafficTypeName}";

            _redisAdapterMock
                .Setup(x => x.Set(ttKey, "1"))
                .Returns(true);

            _redisAdapterMock
                .Setup(x => x.Get(ttKey))
                .Returns("1");

            //Act
            _redisSplitCache.AddSplit(splitName, split);

            //Assert
            var result = (Split)_redisSplitCache.GetSplit(splitName);
        }



        private Split BuildSplit(string splitName)
        {
            return new Split
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
        }
    }
}
