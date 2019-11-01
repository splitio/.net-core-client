using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;

namespace Splitio_Tests.Unit_Tests.InputValidation
{
    [TestClass]
    public class TrafficTypeValidatorTests
    {
        private Mock<ISplitLogger> _log;
        private Mock<ISplitCache> _splitCache;
        private TrafficTypeValidator trafficTypeValidator;

        [TestInitialize]
        public void Initialize()
        {
            _log = new Mock<ISplitLogger>();
            _splitCache = new Mock<ISplitCache>();

            trafficTypeValidator = new TrafficTypeValidator(_splitCache.Object, _log.Object);
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
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Exactly(1));
            _log.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Never);
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
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Exactly(1));
            _log.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void IsValid_WHenTrafficTypeHasCapitalizedLetters_ReturnsTrue()
        {
            // Arrange.
            string trafficType = "aBcDeFg";
            var method = "Tests";

            _splitCache
                .Setup(mock => mock.TrafficTypeExists(trafficType.ToLower()))
                .Returns(true);

            // Act.
            var result = trafficTypeValidator.IsValid(trafficType, method);

            // Asserts.
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(trafficType, result.Value);
            Assert.AreEqual(trafficType.ToLower(), result.Value);
            _log.Verify(mock => mock.Warn($"{method}: {trafficType} should be all lowercase - converting string to lowercase"), Times.Once());
            _log.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Exactly(1));
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void IsValid_WhenTrafficTypeDoesNotExist_ReturnsTrue()
        {
            // Arrange.
            string trafficType = "traffict_type_test";
            var method = "Tests";

            _splitCache
                .Setup(mock => mock.TrafficTypeExists(trafficType))
                .Returns(false);

            // Act.
            var result = trafficTypeValidator.IsValid(trafficType, method);

            // Asserts.
            Assert.IsTrue(result.Success);
            Assert.AreEqual(trafficType, result.Value);
            _log.Verify(mock => mock.Warn($"Track: Traffic Type {trafficType} does not have any corresponding Splits in this environment, make sure you’re tracking your events to a valid traffic type defined in the Split console."), Times.Once());
            _log.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Exactly(1));
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Never);
        }
    }
}
