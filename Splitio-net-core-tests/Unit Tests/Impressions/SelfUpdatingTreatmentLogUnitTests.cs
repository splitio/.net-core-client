using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Shared.Classes;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class SelfUpdatingTreatmentLogUnitTests
    {
        [TestMethod]
        public void LogSuccessfully()
        {
            //Arrange
            var queue = new BlockingQueue<KeyImpression>(10);
            var impressionsCache = new InMemorySimpleCache<KeyImpression>(queue);
            var treatmentLog = new SelfUpdatingTreatmentLog(null, 1, impressionsCache, 10);

            //Act
            var impression = new KeyImpression() { keyName = "GetTreatment", feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test" };
            treatmentLog.Log(impression);

            //Assert
            KeyImpression element = null;
            while (element == null)
            {
                element = queue.Dequeue();
            }
            Assert.IsNotNull(element);
            Assert.AreEqual("GetTreatment", element.keyName);
            Assert.AreEqual("test", element.feature);
            Assert.AreEqual("on", element.treatment);
            Assert.AreEqual(7000, element.time);
        }

        [TestMethod]
        public void LogSuccessfullyUsingBucketingKey()
        {
            //Arrange
            var queue = new BlockingQueue<KeyImpression>(10);
            var impressionsCache = new InMemorySimpleCache<KeyImpression>(queue);
            var treatmentLog = new SelfUpdatingTreatmentLog(null, 1, impressionsCache, 10);

            //Act
            Key key = new Key(bucketingKey : "a", matchingKey : "testkey");
            var impression = new KeyImpression() { keyName = key.matchingKey, feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label", bucketingKey = key.bucketingKey };
            treatmentLog.Log(impression);

            //Assert
            KeyImpression element = null;
            while (element == null)
            {
                element = queue.Dequeue();
            }
            Assert.IsNotNull(element);
            Assert.AreEqual("testkey", element.keyName);
            Assert.AreEqual("a", element.bucketingKey);
            Assert.AreEqual("test", element.feature);
            Assert.AreEqual("on", element.treatment);
            Assert.AreEqual(7000, element.time);
        }

        [TestMethod]
        public void LogSuccessfullyAndSendImpressions()
        {
            //Arrange
            var apiClientMock = new Mock<ITreatmentSdkApiClient>();
            var queue = new BlockingQueue<KeyImpression>(10);
            var impressionsCache = new InMemorySimpleCache<KeyImpression>(queue);
            var treatmentLog = new SelfUpdatingTreatmentLog(apiClientMock.Object, 1, impressionsCache, 10);

            //Act
            treatmentLog.Start();
            var impression = new KeyImpression() { keyName = "GetTreatment", feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label" };
            treatmentLog.Log(impression);

            //Assert
            Thread.Sleep(2000);
            apiClientMock.Verify(x => x.SendBulkImpressions(It.IsAny<string>()));
        }
    }
}
