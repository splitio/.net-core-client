using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.EventSource.Workers;
using Splitio.Services.Logger;
using Splitio.Services.SplitFetcher.Interfaces;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.EventSource.Workers
{
    [TestClass]
    public class SplitsWorkerTests
    {
        private readonly Mock<ISplitLogger> _log;
        private readonly Mock<ISplitFetcher> _splitFetcher;
        private readonly Mock<ISplitCache> _splitCache;

        private readonly ISplitsWorker _splitsWorker;

        public SplitsWorkerTests()
        {
            _log = new Mock<ISplitLogger>();
            _splitFetcher = new Mock<ISplitFetcher>();
            _splitCache = new Mock<ISplitCache>();

            _splitsWorker = new SplitsWorker(_splitFetcher.Object, _splitCache.Object, _log.Object);
        }

        [TestMethod]
        public void AddToQueue_WithElements_ShouldTriggerFetch()
        {
            // Arrange.
            _splitsWorker.Start();
            _splitCache
                .SetupSequence(mock => mock.GetChangeNumber())
                .Returns(1585956698447)
                .Returns(1585956698457)
                .Returns(1585956698467)
                .Returns(1585956698477);

            // Act.            
            _splitsWorker.AddToQueue(1585956698457);
            _splitsWorker.AddToQueue(1585956698467);
            _splitsWorker.AddToQueue(1585956698477);
            Thread.Sleep(50);

            _splitsWorker.AddToQueue(1585956698476);
            Thread.Sleep(500);

            _splitsWorker.Stop();
            _splitsWorker.AddToQueue(1585956698486);
            Thread.Sleep(100);
            _splitsWorker.AddToQueue(1585956698496);
            Thread.Sleep(100);

            // Assert.
            _splitCache.Verify(mock => mock.GetChangeNumber(), Times.Exactly(4));
            _splitFetcher.Verify(mock => mock.Fetch(), Times.Exactly(3));
        }

        [TestMethod]
        public void AddToQueue_WithoutElemts_ShouldNotTriggerFetch()
        {
            // Act.
            _splitsWorker.Start();
            Thread.Sleep(500);

            // Assert.
            _splitCache.Verify(mock => mock.GetChangeNumber(), Times.Never);
            _splitFetcher.Verify(mock => mock.Fetch(), Times.Never);
        }

        [TestMethod]
        public void Kill_ShouldTriggerFethc()
        {
            // Arrange.            
            var changeNumber = 1585956698457;
            var splitName = "split-test";
            var defaultTreatment = "off";

            _splitCache
                .Setup(mock => mock.GetChangeNumber())
                .Returns(1585956698447);

            _splitsWorker.Start();

            // Act.            
            _splitsWorker.KillSplit(changeNumber, splitName, defaultTreatment);
            Thread.Sleep(500);

            // Assert.
            _splitCache.Verify(mock => mock.Kill(changeNumber, splitName, defaultTreatment), Times.Once);
            _splitCache.Verify(mock => mock.GetChangeNumber(), Times.Once);
            _splitFetcher.Verify(mock => mock.Fetch(), Times.Once);
        }

        [TestMethod]
        public void Kill_ShouldNotTriggerFethc()
        {
            // Arrange.            
            var changeNumber = 1585956698457;
            var splitName = "split-test";
            var defaultTreatment = "off";

            _splitCache
                .Setup(mock => mock.GetChangeNumber())
                .Returns(1585956698467);

            _splitsWorker.Start();

            // Act.            
            _splitsWorker.KillSplit(changeNumber, splitName, defaultTreatment);
            Thread.Sleep(500);

            // Assert.
            _splitCache.Verify(mock => mock.Kill(changeNumber, splitName, defaultTreatment), Times.Once);
            _splitCache.Verify(mock => mock.GetChangeNumber(), Times.Once);
            _splitFetcher.Verify(mock => mock.Fetch(), Times.Never);
        }
    }
}
