using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.SplitFetcher.Interfaces;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Common
{
    [TestClass]
    public class SynchronizerTests
    {
        private readonly Mock<ISplitFetcher> _splitFetcher;
        private readonly Mock<ISelfRefreshingSegmentFetcher> _segmentFetcher;
        private readonly Mock<IImpressionsLog> _impressionsLog;
        private readonly Mock<IEventsLog> _eventsLog;
        private readonly Mock<IMetricsLog> _metricsLog;
        private readonly Mock<IReadinessGatesCache> _gates;
        private readonly Mock<ISplitLogger> _log;
        private readonly ISynchronizer _synchronizer;

        public SynchronizerTests()
        {
            _splitFetcher = new Mock<ISplitFetcher>();
            _segmentFetcher = new Mock<ISelfRefreshingSegmentFetcher>();
            _impressionsLog = new Mock<IImpressionsLog>();
            _eventsLog = new Mock<IEventsLog>();
            _metricsLog = new Mock<IMetricsLog>();
            _gates = new Mock<IReadinessGatesCache>();
            _log = new Mock<ISplitLogger>();

            _synchronizer = new Synchronizer(_splitFetcher.Object, _segmentFetcher.Object, _impressionsLog.Object, _eventsLog.Object, _metricsLog.Object, _gates.Object, log: _log.Object);
        }

        [TestMethod]
        public void StartPeriodicDataRecording_ShouldStartServices()
        {
            // Act.
            _synchronizer.StartPeriodicDataRecording();

            // Assert.
            _impressionsLog.Verify(mock => mock.Start(), Times.Once);
            _eventsLog.Verify(mock => mock.Start(), Times.Once);
            _metricsLog.Verify(mock => mock.Start(), Times.Once);
        }

        [TestMethod]
        public void StartPeriodicFetching_ShouldStartFetchings()
        {
            // Arrange.
            _gates
                .Setup(mock => mock.IsSDKReady(0))
                .Returns(true);

            // Act.
            _synchronizer.StartPeriodicFetching();

            // Assert.
            _splitFetcher.Verify(mock => mock.Start(), Times.Once);

            Thread.Sleep(100);
            _segmentFetcher.Verify(mock => mock.Start(), Times.Once);
        }

        [TestMethod]
        public void StopPeriodicDataRecording_ShouldStopServices()
        {
            // Act.
            _synchronizer.StopPeriodicDataRecording();

            // Assert.
            _impressionsLog.Verify(mock => mock.Stop(), Times.Once);
            _eventsLog.Verify(mock => mock.Stop(), Times.Once);
            _metricsLog.Verify(mock => mock.Clear(), Times.Once);
        }

        [TestMethod]
        public void StopPeriodicFetching_ShouldStopFetchings()
        {
            // Act.
            _synchronizer.StopPeriodicFetching();

            // Assert.
            _splitFetcher.Verify(mock => mock.Stop(), Times.Once);
            _segmentFetcher.Verify(mock => mock.Stop(), Times.Once);
        }

        [TestMethod]
        public void SyncAll_ShouldStartFetchSplitsAndSegments()
        {
            // Act.
            _synchronizer.SyncAll();

            // Assert.
            _splitFetcher.Verify(mock => mock.FetchSplits(), Times.Once);
            _segmentFetcher.Verify(mock => mock.FetchSegments(), Times.Once);
        }

        [TestMethod]
        public void SynchorizeSegment_ShouldFetchSegmentByName()
        {
            // Arrange.
            var segmentName = "segment-test";

            // Act.
            _synchronizer.SynchorizeSegment(segmentName);

            // Assert.
            _segmentFetcher.Verify(mock => mock.FetchSegment(segmentName), Times.Once);
        }

        [TestMethod]
        public void SynchorizeSplits_ShouldFetchSplits()
        {
            // Act.
            _synchronizer.SynchorizeSplits();

            // Assert.
            _splitFetcher.Verify(mock => mock.FetchSplits(), Times.Once);
        }
    }
}
