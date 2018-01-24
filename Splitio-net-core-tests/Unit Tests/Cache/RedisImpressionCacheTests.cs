using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
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
            cache.AddItem(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });

            //Assert
            redisAdapterMock.Verify(mock => mock.SAdd(key, It.IsAny<RedisValue>()));
        }
    }
}
