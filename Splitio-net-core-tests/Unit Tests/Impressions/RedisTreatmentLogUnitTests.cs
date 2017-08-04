using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Impressions.Classes;
using Splitio.Domain;
using System.Threading;
using Moq;
using Splitio.Services.Cache.Interfaces;
using Splitio.Redis.Services.Impressions.Classes;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class RedisTreatmentLogUnitTests
    {
        [TestMethod]
        public void LogSuccessfully()
        {
            //Arrange
            var impressionsCache = new Mock<IImpressionsCache>();
            var treatmentLog = new RedisTreatmentLog(impressionsCache.Object);

            //Act
            var impression = new KeyImpression() { keyName = "GetTreatment", feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test" };
            treatmentLog.Log(impression);

            //Assert
            Thread.Sleep(1000);
            impressionsCache.Verify(mock => mock.AddImpression(It.IsAny<KeyImpression>()), Times.Once());
        }

        [TestMethod]
        public void LogSuccessfullyUsingBucketingKey()
        {
            //Arrange
            var impressionsCache = new Mock<IImpressionsCache>();
            var treatmentLog = new RedisTreatmentLog(impressionsCache.Object);

            //Act
            Key key = new Key(bucketingKey : "a", matchingKey : "testkey");
            var impression = new KeyImpression() { keyName = key.matchingKey, feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label", bucketingKey = key.bucketingKey };
            treatmentLog.Log(impression);

            //Assert
            Thread.Sleep(1000);
            impressionsCache.Verify(mock => mock.AddImpression(It.Is<KeyImpression>(p => p.keyName == key.matchingKey && p.feature == "test" && p.treatment == "on" && p.time == 7000 && p.changeNumber == 1 && p.label == "test-label" && p.bucketingKey == key.bucketingKey)), Times.Once());
        }    
    }
}
