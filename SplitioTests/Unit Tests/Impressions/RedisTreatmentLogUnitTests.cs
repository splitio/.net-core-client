using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Impressions.Classes;
using Splitio.Domain;
using System.Threading;
using Moq;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Cache.Interfaces;

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
            treatmentLog.Log("GetTreatment", "test", "on", 7000, 1, "test");

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
            treatmentLog.Log(key.matchingKey, "test", "on", 7000, 1, "test-label", key.bucketingKey);

            //Assert
            Thread.Sleep(1000);
            impressionsCache.Verify(mock => mock.AddImpression(It.IsAny<KeyImpression>()), Times.Once());
        }    
    }
}
