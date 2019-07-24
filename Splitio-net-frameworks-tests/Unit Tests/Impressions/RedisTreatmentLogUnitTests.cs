﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Impressions.Classes;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace Splitio_net_frameworks_tests.Unit_Tests.Impressions
{
    [TestClass]
    public class RedisTreatmentLogUnitTests
    {
        private Mock<ISimpleCache<IList<KeyImpression>>> _impressionsCache;
        private RedisTreatmentLog _redisTreatmentLog;

        [TestInitialize]
        public void Initialization()
        {
            _impressionsCache = new Mock<ISimpleCache<IList<KeyImpression>>>();

            _redisTreatmentLog = new RedisTreatmentLog(_impressionsCache.Object);
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
            _redisTreatmentLog.Log(impressions);

            //Assert
            Thread.Sleep(1000);
            _impressionsCache.Verify(mock => mock.AddItem(It.IsAny<IList<KeyImpression>>()), Times.Once());
        }

        [TestMethod]
        public void LogSuccessfullyUsingBucketingKey()
        {
            //Arrange
            Key key = new Key(bucketingKey: "a", matchingKey: "testkey");

            var impressions = new List<KeyImpression>
            {
                new KeyImpression { keyName = key.matchingKey, feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label", bucketingKey = key.bucketingKey }
            };

            //Act
            _redisTreatmentLog.Log(impressions);

            //Assert
            Thread.Sleep(1000);
            _impressionsCache
                .Verify(mock => mock.AddItem(It.Is<IList<KeyImpression>>(v => v.Any(ki => ki.keyName == key.matchingKey 
                                                                                       && ki.feature == "test" 
                                                                                       && ki.treatment == "on" 
                                                                                       && ki.time == 7000 
                                                                                       && ki.changeNumber == 1 
                                                                                       && ki.label == "test-label" 
                                                                                       && ki.bucketingKey == key.bucketingKey))), Times.Once());
        }
    }
}
