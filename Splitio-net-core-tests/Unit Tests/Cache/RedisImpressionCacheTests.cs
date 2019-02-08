using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class RedisImpressionCacheTests
    {
        [TestMethod]
        public void AddImpressionSuccessfully()
        {
            //Arrange
            var key = "SPLITIO.impressions";
            var redisAdapterMock = new Mock<IRedisAdapter>();
            var cache = new RedisImpressionsCache(redisAdapterMock.Object, "10.0.0.1", "net-1.0.2");
            var impressions = new List<KeyImpression>
            {
                new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 }
            };

            //Act
            cache.AddItem(impressions);

            //Assert
            redisAdapterMock.Verify(mock => mock.ListRightPush(key, It.IsAny<RedisValue>()));
        }
    }
}
