using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Common;
using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Common
{
    [TestClass]
    public class SyncManagerTests
    {
        private readonly Mock<ISynchronizer> _synchronizer;
        private readonly Mock<IPushManager> _pushManager;
        private readonly Mock<ISSEHandler> _sseHandler;
        private readonly Mock<ISplitLogger> _log;
        private ISyncManager _syncManager;

        public SyncManagerTests()
        {
            _synchronizer = new Mock<ISynchronizer>();
            _pushManager = new Mock<IPushManager>();
            _sseHandler = new Mock<ISSEHandler>();
            _log = new Mock<ISplitLogger>();
        }

        [TestMethod]
        public void Start_WithStreamingDisabled_ShouldStartPoll()
        {
            // Arrange.
            var streamingEnabled = false;
            _syncManager = new SyncManager(streamingEnabled, _synchronizer.Object, _pushManager.Object, _sseHandler.Object, _log.Object);

            // Act.
            _syncManager.Start();

            // Assert.
            _synchronizer.Verify(mock => mock.StartPeriodicFetching(), Times.Once);
            _synchronizer.Verify(mock => mock.StartPeriodicDataRecording(), Times.Once);

            Thread.Sleep(200);
            _synchronizer.Verify(mock => mock.SyncAll(), Times.Never);
            _synchronizer.Verify(mock => mock.StartPeriodicDataRecording(), Times.Once);
            _pushManager.Verify(mock => mock.StartSse(), Times.Never);
        }

        [TestMethod]
        public void Start_WithStreamingEnabled_ShouldStartStream()
        {
            // Arrange.
            var streamingEnabled = true;
            _syncManager = new SyncManager(streamingEnabled, _synchronizer.Object, _pushManager.Object, _sseHandler.Object, _log.Object);

            // Act.
            _syncManager.Start();

            // Assert.
            Thread.Sleep(500);
            _synchronizer.Verify(mock => mock.SyncAll(), Times.Once);
            _synchronizer.Verify(mock => mock.StartPeriodicDataRecording(), Times.Once);
            _pushManager.Verify(mock => mock.StartSse(), Times.Once);
            
            _synchronizer.Verify(mock => mock.StartPeriodicFetching(), Times.Never);
            _synchronizer.Verify(mock => mock.StartPeriodicDataRecording(), Times.Once);
        }

        [TestMethod]
        public void Shutdown()
        {
            // Arrange.
            var streamingEnabled = true;
            _syncManager = new SyncManager(streamingEnabled, _synchronizer.Object, _pushManager.Object, _sseHandler.Object, _log.Object);

            // Act.
            _syncManager.Shutdown();

            // Assert.
            _synchronizer.Verify(mock => mock.StopPeriodicFetching(), Times.Once);
            _synchronizer.Verify(mock => mock.StopPeriodicDataRecording(), Times.Once);
            _pushManager.Verify(mock => mock.StopSse(), Times.Once);
        }
    }
}
