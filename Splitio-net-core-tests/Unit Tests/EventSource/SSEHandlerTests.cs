using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.EventSource;
using Splitio.Services.EventSource.Workers;
using Splitio.Services.Logger;

namespace Splitio_Tests.Unit_Tests.EventSource
{
    [TestClass]
    public class SSEHandlerTests
    {
        private readonly Mock<ISplitLogger> _log;
        private readonly Mock<ISplitsWorker> _splitsWorker;
        private readonly Mock<ISegmentsWorker> _segmentsWorker;
        private readonly Mock<INotificationPorcessor> _notificationPorcessor;
        private readonly Mock<IEventSourceClient> _eventSourceClient;
        private readonly ISSEHandler _sseHandler;

        public SSEHandlerTests()
        {
            _log = new Mock<ISplitLogger>();
            _splitsWorker = new Mock<ISplitsWorker>();
            _segmentsWorker = new Mock<ISegmentsWorker>();
            _notificationPorcessor = new Mock<INotificationPorcessor>();
            _eventSourceClient = new Mock<IEventSourceClient>();

            _sseHandler = new SSEHandler("www.fake.com", _splitsWorker.Object, _segmentsWorker.Object, _notificationPorcessor.Object, _log.Object, _eventSourceClient.Object);
        }

        [TestMethod]
        public void Start_ShouldConnect()
        {
            // Arrange.
            var token = "fake-test";
            var channels = "channel-test";

            // Act.
            _sseHandler.Start(token, channels);

            // Assert.
            _eventSourceClient.Verify(mock => mock.Connect(), Times.Once);
        }

        [TestMethod]
        public void Stop_ShouldDisconnect()
        {
            // Act.
            _sseHandler.Stop();

            // Assert.
            _eventSourceClient.Verify(mock => mock.Disconnect(), Times.Once);
        }

        [TestMethod]
        public void StartWorkers_ShouldStartWorkers()
        {
            // Act.
            _sseHandler.StartWorkers();

            // Assert.
            _splitsWorker.Verify(mock => mock.Start(), Times.Once);
            _segmentsWorker.Verify(mock => mock.Start(), Times.Once);
        }

        [TestMethod]
        public void StopWorkers_ShouldStopWorkers()
        {
            // Act.
            _sseHandler.StopWorkers();

            // Assert.
            _splitsWorker.Verify(mock => mock.Stop(), Times.Once);
            _segmentsWorker.Verify(mock => mock.Stop(), Times.Once);
        }
    }
}
