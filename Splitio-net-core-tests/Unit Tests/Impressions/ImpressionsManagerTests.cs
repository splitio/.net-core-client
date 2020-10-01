using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.Impressions.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class ImpressionsManagerTests
    {
        private readonly Mock<IImpressionsObserver> _impressionsObserver;
        private readonly Mock<IImpressionsLog> _impressionsLog;
        private readonly Mock<IImpressionListener> _customerImpressionListener;
        private readonly Mock<IImpressionsCounter> _impressionsCounter;

        public ImpressionsManagerTests()
        {
            _impressionsObserver = new Mock<IImpressionsObserver>();
            _impressionsLog = new Mock<IImpressionsLog>();
            _customerImpressionListener = new Mock<IImpressionListener>();
            _impressionsCounter = new Mock<IImpressionsCounter>();
        }

        [TestMethod]
        public void BuildImpressionWithOptimizedAndWithPreviousTime()
        {
            // Arrange.
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, _customerImpressionListener.Object, _impressionsCounter.Object, true, ImpressionModes.Optimized, _impressionsObserver.Object);
            var impTime = CurrentTimeHelper.CurrentTimeMillis();
            var ptTime = impTime - 150;

            _impressionsObserver
                .Setup(mock => mock.TestAndSet(It.IsAny<KeyImpression>()))
                .Returns(ptTime);

            // Act.
            var result = impressionsManager.BuildImpression("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key");

            // Assert.
            Assert.AreEqual("matching-key", result.keyName);
            Assert.AreEqual("feature", result.feature);
            Assert.AreEqual("off", result.treatment);
            Assert.AreEqual(impTime, result.time);
            Assert.AreEqual("label", result.label);
            Assert.AreEqual("bucketing-key", result.bucketingKey);
            Assert.AreEqual(ptTime, result.previousTime);

            _impressionsObserver.Verify(mock => mock.TestAndSet(It.IsAny<KeyImpression>()), Times.Once);
            _impressionsCounter.Verify(mock => mock.Inc("feature", impTime), Times.Once);
        }

        [TestMethod]
        public void BuildImpressionWithDebugAndWithPreviousTime()
        {
            // Arrange.
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, _customerImpressionListener.Object, _impressionsCounter.Object, true, ImpressionModes.Debug, _impressionsObserver.Object);
            var impTime = CurrentTimeHelper.CurrentTimeMillis();

            _impressionsObserver
                .Setup(mock => mock.TestAndSet(It.IsAny<KeyImpression>()))
                .Returns((long?)null);

            // Act.
            var result = impressionsManager.BuildImpression("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key");

            // Assert.
            Assert.AreEqual("matching-key", result.keyName);
            Assert.AreEqual("feature", result.feature);
            Assert.AreEqual("off", result.treatment);
            Assert.AreEqual(impTime, result.time);
            Assert.AreEqual("label", result.label);
            Assert.AreEqual("bucketing-key", result.bucketingKey);
            Assert.IsNull(result.previousTime);

            _impressionsObserver.Verify(mock => mock.TestAndSet(It.IsAny<KeyImpression>()), Times.Once);
            _impressionsCounter.Verify(mock => mock.Inc("feature", impTime), Times.Never);
        }

        [TestMethod]
        public void BuildImpressionWithDebugAndWithoutPreviousTime()
        {
            // Arrange.
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, _customerImpressionListener.Object, _impressionsCounter.Object, false, ImpressionModes.Debug, _impressionsObserver.Object);
            var impTime = CurrentTimeHelper.CurrentTimeMillis();

            // Act.
            var result = impressionsManager.BuildImpression("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key");

            // Assert.
            Assert.AreEqual("matching-key", result.keyName);
            Assert.AreEqual("feature", result.feature);
            Assert.AreEqual("off", result.treatment);
            Assert.AreEqual(impTime, result.time);
            Assert.AreEqual("label", result.label);
            Assert.AreEqual("bucketing-key", result.bucketingKey);
            Assert.IsNull(result.previousTime);

            _impressionsObserver.Verify(mock => mock.TestAndSet(It.IsAny<KeyImpression>()), Times.Never);
            _impressionsCounter.Verify(mock => mock.Inc("feature", impTime), Times.Never);
        }

        [TestMethod]
        public void BuildAndTrack()
        {
            // Arrange.
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, _customerImpressionListener.Object, _impressionsCounter.Object, true, ImpressionModes.Optimized, _impressionsObserver.Object);
            var impTime = CurrentTimeHelper.CurrentTimeMillis();

            // Act.
            impressionsManager.BuildAndTrack("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key");
           
            // Assert.
            _impressionsObserver.Verify(mock => mock.TestAndSet(It.IsAny<KeyImpression>()), Times.Once);
            _impressionsCounter.Verify(mock => mock.Inc("feature", impTime), Times.Once);

            Thread.Sleep(1000);
            _impressionsLog.Verify(mock => mock.Log(It.IsAny<List<KeyImpression>>()), Times.Once);
            _customerImpressionListener.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Once);
        }

        [TestMethod]
        public void BuildAndTrackWithoutCustomerListener()
        {
            // Arrange.
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, null, _impressionsCounter.Object, true, ImpressionModes.Optimized, _impressionsObserver.Object);
            var impTime = CurrentTimeHelper.CurrentTimeMillis();

            // Act.
            impressionsManager.BuildAndTrack("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key");

            // Assert.
            _impressionsObserver.Verify(mock => mock.TestAndSet(It.IsAny<KeyImpression>()), Times.Once);
            _impressionsCounter.Verify(mock => mock.Inc("feature", impTime), Times.Once);

            Thread.Sleep(1000);
            _impressionsLog.Verify(mock => mock.Log(It.IsAny<List<KeyImpression>>()), Times.Once);
            _customerImpressionListener.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Never);
        }

        [TestMethod]
        public void Track()
        {
            // Arrange.
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, _customerImpressionListener.Object, _impressionsCounter.Object, true, ImpressionModes.Optimized, _impressionsObserver.Object);
            var impTime = CurrentTimeHelper.CurrentTimeMillis();
            var impressions = new List<KeyImpression>
            {
                new KeyImpression("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key"),
                new KeyImpression("matching-key-2", "feature-2", "off", impTime, 432543, "label-2", "bucketing-key")
            };

            // Act.
            impressionsManager.Track(impressions);

            // Assert.
            Thread.Sleep(1000);
            _impressionsLog.Verify(mock => mock.Log(impressions), Times.Once);
            _customerImpressionListener.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TrackWithoutCustomerListener_Optimized()
        {
            // Arrange.
            var impressionsObserver = new ImpressionsObserver(new ImpressionHasher());
            var impressionsCounter = new ImpressionsCounter();
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, null, impressionsCounter, true, ImpressionModes.Optimized, impressionsObserver);

            var impTime = CurrentTimeHelper.CurrentTimeMillis();
            var impressions = new List<KeyImpression>
            {
                impressionsManager.BuildImpression("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key"),
                impressionsManager.BuildImpression("matching-key-2", "feature-2", "off", impTime, 432543, "label-2", "bucketing-key"),
                impressionsManager.BuildImpression("matching-key-2", "feature-2", "off", impTime, 432543, "label-2", "bucketing-key"),
                impressionsManager.BuildImpression("matching-key-2", "feature-2", "off", impTime, 432543, "label-2", "bucketing-key")
            };

            var optimizedImpressions = impressions.Where(i => impressionsManager.ShouldQueueImpression(i)).ToList();

            // Act.
            impressionsManager.Track(impressions);

            // Assert.
            Thread.Sleep(1000);
            Assert.AreEqual(2, optimizedImpressions.Count());
            _impressionsLog.Verify(mock => mock.Log(optimizedImpressions), Times.Once);
            _impressionsLog.Verify(mock => mock.Log(impressions), Times.Never);
            _customerImpressionListener.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Never);
        }

        [TestMethod]
        public void TrackWithoutCustomerListener_Debug()
        {
            // Arrange.
            var impressionsObserver = new ImpressionsObserver(new ImpressionHasher());
            var impressionsCounter = new ImpressionsCounter();
            var impressionsManager = new ImpressionsManager(_impressionsLog.Object, null, impressionsCounter, true, ImpressionModes.Debug, impressionsObserver);

            var impTime = CurrentTimeHelper.CurrentTimeMillis();
            var impressions = new List<KeyImpression>
            {
                impressionsManager.BuildImpression("matching-key", "feature", "off", impTime, 432543, "label", "bucketing-key"),
                impressionsManager.BuildImpression("matching-key-2", "feature-2", "off", impTime, 432543, "label-2", "bucketing-key"),
                impressionsManager.BuildImpression("matching-key-2", "feature-2", "off", impTime, 432543, "label-2", "bucketing-key"),
                impressionsManager.BuildImpression("matching-key-2", "feature-2", "off", impTime, 432543, "label-2", "bucketing-key")
            };

            // Act.
            impressionsManager.Track(impressions);

            // Assert.
            Thread.Sleep(1000);
            _impressionsLog.Verify(mock => mock.Log(impressions), Times.Once);
            _customerImpressionListener.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Never);
        }
    }
}
