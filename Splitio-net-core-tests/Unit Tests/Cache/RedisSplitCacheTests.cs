using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Parsing.Interfaces;
using StackExchange.Redis;
using System;
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
        private readonly Mock<ISplitParser> _splitParserMock;
        private readonly RedisSplitCache _redisSplitCache;

        public RedisSplitCacheTests()
        {
            _redisAdapterMock = new Mock<IRedisAdapter>();
            _splitParserMock = new Mock<ISplitParser>();

            _redisSplitCache = new RedisSplitCache(_redisAdapterMock.Object, _splitParserMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void AddSplit_ReturnsNotImplementedException()
        {
            //Act
            _redisSplitCache.AddSplit("splitName", new Split());
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
        [ExpectedException(typeof(NotImplementedException))]
        public void RemoveSplit_ReturnsNotImplementedException()
        {
            //Act
            var isRemoved = _redisSplitCache.RemoveSplit("splitName");
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void SetChangeNumber__ReturnsNotImplementedException()
        {
            //Act
            _redisSplitCache.SetChangeNumber(changeNumber: 123);
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
            var splitJson2 = JsonConvert.SerializeObject(split2);

            _redisAdapterMock
                .Setup(x => x.Keys(splitKeyPrefix + "*"))
                .Returns(new RedisKey[] { "test_split", "test_split2" });

            _redisAdapterMock
                .Setup(x => x.Get(It.IsAny<RedisKey[]>()))
                .Returns(new RedisValue[] { splitJson, splitJson2 });

            _splitParserMock
                .Setup(mock => mock.Parse(It.IsAny<Split>()))
                .Returns(new ParsedSplit());                

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
        [ExpectedException(typeof(NotImplementedException))]
        public void FlushTest()
        {
            //Act
            _redisSplitCache.Flush();
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

        #region TrafficTypeExists
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
        #endregion

        [TestMethod]
        public void FetchMany_VerifyMGetCall_Once()
        {
            // Arrange.
            var splitNames = new List<string> { "Split_1", "Split_2", "Split_3" };

            _redisAdapterMock
                .Setup(mock => mock.Get(It.IsAny<RedisKey[]>()))
                .Returns(new RedisValue[3]);

            // Act.
            var result = _redisSplitCache.FetchMany(splitNames);

            // Assert.
            _redisAdapterMock.Verify(mock => mock.Get(It.IsAny<RedisKey[]>()), Times.Once);
            _redisAdapterMock.Verify(mock => mock.Get(It.IsAny<string>()), Times.Never);
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
