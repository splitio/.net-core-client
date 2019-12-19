using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Impressions.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class RedisImpressionsLogUnitTests
    {
        private Mock<ISimpleCache<KeyImpression>> _impressionsCache;
        private RedisImpressionLog _redisImpressionLog;

        [TestInitialize]
        public void Initialization()
        {
            _impressionsCache = new Mock<ISimpleCache<KeyImpression>>();

            _redisImpressionLog = new RedisImpressionLog(_impressionsCache.Object);
        }

        [TestMethod]
        public void LogSuccessfully()
        {
            //Arrange
            var impressions = new List<KeyImpression>
            {
                new KeyImpression { keyName = "GetTreatment", feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test" }
            };

            //Act
            _redisImpressionLog.Log(impressions);

            //Assert
            _impressionsCache.Verify(mock => mock.AddItems(It.IsAny<IList<KeyImpression>>()), Times.Once());
        }

        [TestMethod]
        public void LogSuccessfullyUsingBucketingKey()
        {
            //Arrange
            var key = new Key(bucketingKey: "a", matchingKey: "testkey");

            var impressions = new List<KeyImpression>
            {
                new KeyImpression { keyName = key.matchingKey, feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label", bucketingKey = key.bucketingKey }
            };

            //Act
            _redisImpressionLog.Log(impressions);

            //Assert
            _impressionsCache
                .Verify(mock => mock.AddItems(It.Is<IList<KeyImpression>>(v => v.Any(ki => ki.keyName == key.matchingKey
                                                                                       && ki.feature == "test"
                                                                                       && ki.treatment == "on"
                                                                                       && ki.time == 7000
                                                                                       && ki.changeNumber == 1
                                                                                       && ki.label == "test-label"
                                                                                       && ki.bucketingKey == key.bucketingKey))), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Start_ReturnsException()
        {
            //Act
            _redisImpressionLog.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Stop_ReturnsException()
        {
            //Act
            _redisImpressionLog.Stop();
        }
    }
}
