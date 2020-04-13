using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Common;
using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Common
{
    [TestClass]
    public class PushManagerTests
    {
        private const int AuthRetryBackOffBase = 1000;

        private readonly Mock<IAuthApiClient> _authApiClient;
        private readonly Mock<ISplitLogger> _log;
        private readonly Mock<ISSEHandler> _sseHandler;
        private readonly IPushManager _pushManager;

        public PushManagerTests()
        {
            _authApiClient = new Mock<IAuthApiClient>();
            _log = new Mock<ISplitLogger>();
            _sseHandler = new Mock<ISSEHandler>();

            _pushManager = new PushManager(AuthRetryBackOffBase, _sseHandler.Object, _authApiClient.Object, log: _log.Object);
        }

        [TestMethod]
        public void StartSse_WithPushEnabled_ShouldConnect()
        {
            // Arrange.
            var response = new AuthenticationResponse
            {
                PushEnabled = true,
                Channels = "channel-test",
                Token = "token-test",
                Retry = false,
                Expiration = 1000
            };

            var response2 = new AuthenticationResponse
            {
                PushEnabled = true,
                Channels = "channel-test-2",
                Token = "token-test-2",
                Retry = false,
                Expiration = 1000
            };

            _authApiClient
                .SetupSequence(mock => mock.AuthenticateAsync())
                .ReturnsAsync(response)
                .ReturnsAsync(response2);

            // Act.
            _pushManager.StartSse();

            // Assert.
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Once);
            _sseHandler.Verify(mock => mock.Start(response.Token, response.Channels), Times.Once);

            Thread.Sleep(1100);
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Exactly(2));
            _sseHandler.Verify(mock => mock.Start(response2.Token, response2.Channels), Times.Once);
        }

        [TestMethod]
        public void StartSse_WithPushDisable_ShouldNotConnect()
        {
            // Arrange.
            var response = new AuthenticationResponse
            {
                PushEnabled = false,
                Retry = false
            };

            _authApiClient
                .Setup(mock => mock.AuthenticateAsync())
                .ReturnsAsync(response);

            // Act.
            _pushManager.StartSse();

            // Assert.
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Once);
            _sseHandler.Verify(mock => mock.Start(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sseHandler.Verify(mock => mock.Stop(), Times.Once);

            Thread.Sleep(1100);
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Once);
            _sseHandler.Verify(mock => mock.Start(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sseHandler.Verify(mock => mock.Stop(), Times.Once);
        }

        [TestMethod]
        public void StartSse_WithPushDisableAndRetryTrue_ShouldNotConnect()
        {
            // Arrange.
            var response = new AuthenticationResponse
            {
                PushEnabled = false,
                Retry = true
            };

            var response2 = new AuthenticationResponse
            {
                PushEnabled = true,
                Channels = "channel-test-2",
                Token = "token-test-2",
                Retry = false,
                Expiration = 1000
            };

            _authApiClient
                .SetupSequence(mock => mock.AuthenticateAsync())
                .ReturnsAsync(response)
                .ReturnsAsync(response2);

            // Act.
            _pushManager.StartSse();

            // Assert.
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Once);
            _sseHandler.Verify(mock => mock.Start(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sseHandler.Verify(mock => mock.Stop(), Times.Once);

            Thread.Sleep(1500);
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Exactly(2));
            _sseHandler.Verify(mock => mock.Start(response2.Token, response2.Channels), Times.Once);
        }
    }
}
