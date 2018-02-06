using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Shared.Classes;
using System.Collections.Generic;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class SelfUpdatingTreatmentLogUnitTests
    {
        private Mock<ITreatmentSdkApiClient> _apiClientMock;
        private BlockingQueue<KeyImpression> _queue;
        private InMemorySimpleCache<KeyImpression> _impressionsCache;
        private SelfUpdatingTreatmentLog _treatmentLog;

        [TestInitialize]
        public void Initialize()
        {
            _apiClientMock = new Mock<ITreatmentSdkApiClient>();
            _queue = new BlockingQueue<KeyImpression>(10);
            _impressionsCache = new InMemorySimpleCache<KeyImpression>(_queue);
            _treatmentLog = new SelfUpdatingTreatmentLog(_apiClientMock.Object, 1, _impressionsCache, 10);
        }

        [TestMethod]
        public void LogSuccessfully()
        {
            //Act
            var impression = new KeyImpression() { keyName = "GetTreatment", feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test" };
            _treatmentLog.Log(impression);

            //Assert
            KeyImpression element = null;
            while (element == null)
            {
                element = _queue.Dequeue();
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
            //Act
            Key key = new Key(bucketingKey : "a", matchingKey : "testkey");
            var impression = new KeyImpression() { keyName = key.matchingKey, feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label", bucketingKey = key.bucketingKey };
            _treatmentLog.Log(impression);

            //Assert
            KeyImpression element = null;
            while (element == null)
            {
                element = _queue.Dequeue();
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
            //Act
            _treatmentLog.Start();
            var impression = new KeyImpression() { keyName = "GetTreatment", feature = "test", treatment = "on", time = 7000, changeNumber = 1, label = "test-label" };
            _treatmentLog.Log(impression);

            //Assert
            Thread.Sleep(2000);
            _apiClientMock.Verify(x => x.SendBulkImpressions(It.Is<List<KeyImpression>>(list => list.Count == 1)));
        }
    }
}
