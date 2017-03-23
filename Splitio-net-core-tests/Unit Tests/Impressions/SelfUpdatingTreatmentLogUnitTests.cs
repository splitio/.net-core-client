using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Impressions.Classes;
using Splitio.Domain;
using System.Threading;
using Moq;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Cache.Classes;

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
            var impressionsCache = new InMemoryImpressionsCache(queue);
            var treatmentLog = new SelfUpdatingTreatmentLog(null, 1, impressionsCache, 10);

            //Act
            treatmentLog.Log("GetTreatment", "test", "on", 7000, 1, "test");

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
            var impressionsCache = new InMemoryImpressionsCache(queue);
            var treatmentLog = new SelfUpdatingTreatmentLog(null, 1, impressionsCache, 10);

            //Act
            Key key = new Key(bucketingKey : "a", matchingKey : "testkey");
            treatmentLog.Log(key.matchingKey, "test", "on", 7000, 1, "test-label", key.bucketingKey);

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
            var impressionsCache = new InMemoryImpressionsCache(queue);
            var treatmentLog = new SelfUpdatingTreatmentLog(apiClientMock.Object, 1, impressionsCache, 10);

            //Act
            treatmentLog.Start();
            treatmentLog.Log("GetTreatment", "test", "on", 7000, 1, "test-label");

            //Assert
            Thread.Sleep(2000);
            apiClientMock.Verify(x => x.SendBulkImpressions(It.IsAny<string>()));
        }
    }
}
