using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Common;
using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Common
{
    [TestClass]
    public class PushManagerTests
    {
        private readonly Mock<IAuthApiClient> _authApiClient;
        private readonly Mock<ISplitLogger> _log;
        private readonly Mock<IWrapperAdapter> _wrapperAdapter;
        private readonly Mock<ISSEHandler> _sseHandler;
        private readonly IPushManager _pushManager;

        public PushManagerTests()
        {
            _authApiClient = new Mock<IAuthApiClient>();
            _log = new Mock<ISplitLogger>();
            _wrapperAdapter = new Mock<IWrapperAdapter>();
            _sseHandler = new Mock<ISSEHandler>();

            _pushManager = new PushManager(5, _sseHandler.Object, _authApiClient.Object, _log.Object, _wrapperAdapter.Object);
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
                Expiration = 6000
            };

            _authApiClient
                .Setup(mock => mock.AuthenticateAsync())
                .ReturnsAsync(response);

            // Act.
            _pushManager.StartSse();

            // Assert.
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Once);
            _sseHandler.Verify(mock => mock.Start(response.Token, response.Channels), Times.Once);

            // TODO: validate ScheduleNextTokenRefresh
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

            // TODO: validate ScheduleNextTokenRefresh
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

            _authApiClient
                .Setup(mock => mock.AuthenticateAsync())
                .ReturnsAsync(response);

            // Act.
            _pushManager.StartSse();

            // Assert.
            _authApiClient.Verify(mock => mock.AuthenticateAsync(), Times.Once);
            _sseHandler.Verify(mock => mock.Start(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sseHandler.Verify(mock => mock.Stop(), Times.Once);

            // TODO: validate ScheduleNextTokenRefresh
        }
    }
}
