using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Cache.Classes;
using Moq;
using StackExchange.Redis;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class RedisImpressionCacheTests
    {
        private const string impressionKeyPrefix = "SPLITIO/net-1.0.2/10.0.0.1/impressions.";

        [TestMethod]
        public void AddImpressionSuccessfully()
        {
            //Arrange
            var key = impressionKeyPrefix + "test";
            var redisAdapterMock = new Mock<IRedisAdapter>();
            var cache = new RedisImpressionsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

            //Act
            cache.AddImpression(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });

            //Assert
            redisAdapterMock.Verify(mock => mock.SAdd(key, It.IsAny<RedisValue>()));
        }

        [TestMethod]
        public void FetchAllAndClearSuccessfully()
        {        
            //Arrange
            var redisAdapterMock = new Mock<IRedisAdapter>();
            var redisValue = (RedisValue)"{\"keyName\":\"date\",\"treatment\":null,\"time\":10000000,\"changeNumber\":100,\"label\":\"testdate\",\"bucketingKey\":null}";
            redisAdapterMock.Setup(x => x.Keys(impressionKeyPrefix + "*")).Returns(new RedisKey[] { impressionKeyPrefix + "test" });
            redisAdapterMock.Setup(x => x.SMembers(impressionKeyPrefix + "test")).Returns(new RedisValue[]{redisValue});
            redisAdapterMock.Setup(x => x.Del(impressionKeyPrefix + "test")).Returns(true);
            var cache = new RedisImpressionsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");

            //Act
            var result = cache.FetchAllAndClear();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            redisAdapterMock.Verify(mock => mock.Keys(impressionKeyPrefix + "*"));
            redisAdapterMock.Verify(mock => mock.SMembers(impressionKeyPrefix + "test"));
            redisAdapterMock.Verify(mock => mock.Del(impressionKeyPrefix + "test"));
        }
    }
}
