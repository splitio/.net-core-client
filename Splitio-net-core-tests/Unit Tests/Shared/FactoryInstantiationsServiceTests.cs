using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Shared.Classes;

namespace Splitio_Tests.Unit_Tests.Shared
{
    [TestClass]
    public class FactoryInstantiationsServiceTests
    {
        private readonly Mock<ILog> _logMock;

        private FactoryInstantiationsService _factoryInstantiationsService;

        public FactoryInstantiationsServiceTests()
        {
            _logMock = new Mock<ILog>();

            _factoryInstantiationsService = (FactoryInstantiationsService)FactoryInstantiationsService.Instance(_logMock.Object);
        }

        [TestMethod]
        public void FactoryInstantiationsService_AllScenarios()
        {
            // ############################################################
            // #############  Increase_WhenApiKeyDoesntExist  #############
            // ############################################################

            // Arrange
            var apiKey = "apiKey";

            // Act
            _factoryInstantiationsService.Increase(apiKey);
            var result = _factoryInstantiationsService.GetInstantiations();

            // Assert
            Assert.AreEqual(1, result[apiKey]);
            _logMock.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Never);

            // #######################################################
            // #############  Increase_WhenApiKeyExists  #############
            // #######################################################

            // Act
            _factoryInstantiationsService.Increase(apiKey);
            result = _factoryInstantiationsService.GetInstantiations();

            // Assert
            Assert.AreEqual(2, result[apiKey]);
            _logMock.Verify(mock => mock.Warn("factory instantiation: You already have 1 factories with this API Key. We recommend keeping only one instance of the factory at all times(Singleton pattern) and reusing it throughout your application."), Times.Once);

            // ######################################################
            // #############  Increase_WhenIsNewApiKey  #############
            // ######################################################

            // Arrange
            var newApiKey = "newApiKey";

            // Act
            _factoryInstantiationsService.Increase(newApiKey);
            result = _factoryInstantiationsService.GetInstantiations();

            // Assert
            Assert.AreEqual(1, result[newApiKey]);
            _logMock.Verify(mock => mock.Warn("factory instantiation: You already have an instance of the Split factory. Make sure you definitely want this additional instance. We recommend keeping only one instance of the factory at all times(Singleton pattern) and reusing it throughout your application."), Times.Once);

            // ####################################################################
            // #############  Increase_WhenApiKeyExists_ThanMoreOnce  #############
            // ####################################################################

            // Act
            _factoryInstantiationsService.Increase(apiKey);
            result = _factoryInstantiationsService.GetInstantiations();

            // Assert
            Assert.AreEqual(3, result[apiKey]);
            _logMock.Verify(mock => mock.Warn("factory instantiation: You already have 2 factories with this API Key. We recommend keeping only one instance of the factory at all times(Singleton pattern) and reusing it throughout your application."), Times.Once);

            // #######################################################
            // #############  Decrease_WhenApiKeyExists  #############
            // #######################################################

            // Act
            _factoryInstantiationsService.Decrease(apiKey);
            result = _factoryInstantiationsService.GetInstantiations();

            // Assert
            Assert.AreEqual(2, result[apiKey]);

            // Act
            _factoryInstantiationsService.Decrease(apiKey);
            result = _factoryInstantiationsService.GetInstantiations();

            // Assert
            Assert.AreEqual(1, result[apiKey]);

            // ####################################################################################
            // #############  Decrease_WhenApiKeyExists_AndIsTheLastOne_ReturnsFalse  #############
            // ####################################################################################

            // Act
            _factoryInstantiationsService.Decrease(apiKey);
            result = _factoryInstantiationsService.GetInstantiations();

            // Assert
            Assert.IsFalse(result.TryGetValue(apiKey, out int value));
        }
    }
}