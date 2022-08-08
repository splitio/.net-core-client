using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.EventSource.Workers;
using Splitio.Services.Logger;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.EventSource.Workers
{
    [TestClass]
    public class SegmentsWorkerTests
    {
        private readonly Mock<ISplitLogger> _log;
        private readonly Mock<ISegmentCache> _segmentCache;
        private readonly Mock<ISynchronizer> _synchronizer;

        private readonly ISegmentsWorker _segmentsWorker;

        public SegmentsWorkerTests()
        {
            _log = new Mock<ISplitLogger>();
            _segmentCache = new Mock<ISegmentCache>();
            _synchronizer = new Mock<ISynchronizer>();

            _segmentsWorker = new SegmentsWorker(_segmentCache.Object, _synchronizer.Object, _log.Object);
        }

        [TestMethod]
        public void AddToQueue_WithElements_ShouldTriggerFetch()
        {
            // Arrange.
            var changeNumber = 1585956698457;
            var segmentName = "segment-test";

            _segmentCache
                .SetupSequence(mock => mock.GetChangeNumber(segmentName))
                .Returns(1585956698447)
                .Returns(1585956698458);

            var changeNumber2 = 1585956698467;
            var segmentName2 = "segment-test-2";

            _segmentCache
                .SetupSequence(mock => mock.GetChangeNumber(segmentName2))
                .Returns(changeNumber)
                .Returns(1585956698478);

            var changeNumber3 = 1585956698477;
            var segmentName3 = "segment-test-3";

            _segmentCache
                .SetupSequence(mock => mock.GetChangeNumber(segmentName3))
                .Returns(changeNumber3 + 1)
                .Returns(1585956698488);

            _segmentsWorker.Start();

            // Act.
            _segmentsWorker.AddToQueue(changeNumber, segmentName);
            _segmentsWorker.AddToQueue(changeNumber2, segmentName2);
            _segmentsWorker.AddToQueue(changeNumber3, segmentName3);
            Thread.Sleep(1000);

            _segmentsWorker.Stop();
            _segmentsWorker.AddToQueue(1585956698487, "segment-test-4");
            Thread.Sleep(10);

            // Assert.
            _segmentCache.Verify(mock => mock.GetChangeNumber(It.IsAny<string>()), Times.Exactly(5));
            _synchronizer.Verify(mock => mock.SynchronizeSegment(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void AddToQueue_MaxAttemptsAllowed()
        {
            // Arrange.
            var changeNumber = 1585956698457;
            var segmentName = "segment-test";

            _segmentCache
                .Setup(mock => mock.GetChangeNumber(segmentName))
                .Returns(1585956698447);

            _segmentsWorker.Start();

            // Act.
            _segmentsWorker.AddToQueue(changeNumber, segmentName);
            Thread.Sleep(1000);

            // Assert.
            _segmentCache.Verify(mock => mock.GetChangeNumber(segmentName), Times.Exactly(11));
            _synchronizer.Verify(mock => mock.SynchronizeSegment(segmentName), Times.Exactly(10));
        }

        [TestMethod]
        public void AddToQueue_WithoutElemts_ShouldNotTriggerFetch()
        {
            // Act.
            _segmentsWorker.Start();
            Thread.Sleep(500);

            // Assert.
            _segmentCache.Verify(mock => mock.GetChangeNumber(It.IsAny<string>()), Times.Never);
            _synchronizer.Verify(mock => mock.SynchronizeSegment(It.IsAny<string>()), Times.Never);
        }
    }
}
