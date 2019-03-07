using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.InputValidation.Classes;

namespace Splitio_Tests.Unit_Tests.InputValidation
{
    [TestClass]
    public class TrafficTypeValidatorTests
    {
        private Mock<ILog> _log;
        private TrafficTypeValidator trafficTypeValidator;

        [TestInitialize]
        public void Initialize()
        {
            _log = new Mock<ILog>();

            trafficTypeValidator = new TrafficTypeValidator(_log.Object);
        }

        [TestMethod]
        public void IsValid_WHenTrafficTypeIsNull_ReturnsFalse()
        {
            // Arrange.
            string trafficType = null;
            var method = "Tests";

            // Act.
            var result = trafficTypeValidator.IsValid(trafficType, method);

            // Asserts.
            Assert.IsFalse(result.Success);
            _log.Verify(mock => mock.Error($"{method}: you passed a null traffic_type, traffic_type must be a non-empty string"), Times.Once());
        }

        [TestMethod]
        public void IsValid_WHenTrafficTypeIsEmpty_ReturnsFalse()
        {
            // Arrange.
            var trafficType = string.Empty;
            var method = "Tests";

            // Act.
            var result = trafficTypeValidator.IsValid(trafficType, method);

            // Asserts.
            Assert.IsFalse(result.Success);
            _log.Verify(mock => mock.Error($"{method}: you passed an empty traffic_type, traffic_type must be a non-empty string"), Times.Once());
        }

        [TestMethod]
        public void IsValid_WHenTrafficTypeHasCapitalizedLetters_ReturnsTrue()
        {
            // Arrange.
            string trafficType = "aBcDeFg";
            var method = "Tests";

            // Act.
            var result = trafficTypeValidator.IsValid(trafficType, method);

            // Asserts.
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(trafficType, result.Value);
            Assert.AreEqual("abcdefg", result.Value);
            _log.Verify(mock => mock.Warn($"{method}: {trafficType} should be all lowercase - converting string to lowercase"), Times.Once());
        }
    }
}
