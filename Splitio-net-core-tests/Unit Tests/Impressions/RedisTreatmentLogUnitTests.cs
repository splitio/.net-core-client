using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Impressions.Classes;
using Splitio.Services.Shared.Interfaces;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class RedisTreatmentLogUnitTests
    {
        [TestMethod]
        public void LogSuccessfully()
        {
            //Arrange
            var impressionsCache = new Mock<ISimpleCache<KeyImpression>>();
            var treatmentLog = new RedisTreatmentLog(impressionsCache.Object);

            //Act
            var impression = new KeyImpression() { keyName = "GetTreatment", feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test" };
            treatmentLog.Log(impression);

            //Assert
            Thread.Sleep(1000);
            impressionsCache.Verify(mock => mock.AddItem(It.IsAny<KeyImpression>()), Times.Once());
        }

        [TestMethod]
        public void LogSuccessfullyUsingBucketingKey()
        {
            //Arrange
            var impressionsCache = new Mock<ISimpleCache<KeyImpression>>();
            var treatmentLog = new RedisTreatmentLog(impressionsCache.Object);

            //Act
            Key key = new Key(bucketingKey : "a", matchingKey : "testkey");
            var impression = new KeyImpression() { keyName = key.matchingKey, feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label", bucketingKey = key.bucketingKey };
            treatmentLog.Log(impression);

            //Assert
            Thread.Sleep(1000);
            impressionsCache.Verify(mock => mock.AddItem(It.Is<KeyImpression>(p => p.keyName == key.matchingKey && p.feature == "test" && p.treatment == "on" && p.time == 7000 && p.changeNumber == 1 && p.label == "test-label" && p.bucketingKey == key.bucketingKey)), Times.Once());
        }    
    }
}
