using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.InputValidation.Classes;

namespace Splitio_Tests.Unit_Tests.InputValidation
{
    [TestClass]
    public class ApiKeyValidatorTests
    {
        private Mock<ILog> _log;
        private ApiKeyValidator apiKeyValidator;

        [TestInitialize]
        public void Initialize()
        {
            _log = new Mock<ILog>();

            apiKeyValidator = new ApiKeyValidator(_log.Object);
        }

        [TestMethod]
        public void Validate_WhenApiKeyIsEmpty_LogOneError()
        {
            //Arrange
            var apiKey = string.Empty;

            //Act
            apiKeyValidator.Validate(apiKey);

            //Assert
            _log.Verify(mock => mock.Error($"factory instantiation: you passed and empty api_key, api_key must be a non-empty string."), Times.Once());
        }

        [TestMethod]
        public void Validate_WhenApiKeyIsNull_LogOneError()
        {
            //Arrange
            string apiKey = null;

            //Act
            apiKeyValidator.Validate(apiKey);

            //Assert
            _log.Verify(mock => mock.Error($"factory instantiation: you passed a null api_key, api_key must be a non-empty string."), Times.Once());
        }

        [TestMethod]
        public void Validate_WhenApiKeyHasValue_NoLog()
        {
            //Arrange
            string apiKey = "api_key";

            //Act
            apiKeyValidator.Validate(apiKey);

            //Assert
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Never());
        }
    }
}
