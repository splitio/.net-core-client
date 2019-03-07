using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.InputValidation.Classes;

namespace Splitio_Tests.Unit_Tests.InputValidation
{
    [TestClass]
    public class KeyValidatorTests
    {
        private Mock<ILog> _log;
        private KeyValidator keyValidator;

        [TestInitialize]
        public void Initialize()
        {
            _log = new Mock<ILog>();

            keyValidator = new KeyValidator(_log.Object);
        }

        [TestMethod]
        public void IsValid_WhenMatchingKeyAndBucketingKeyAreNull_ReturnsFalse()
        {
            // Arrange. 
            var key = new Key(null, null);
            var method = "Test";

            // Act.
            var result = keyValidator.IsValid(key, method);

            // Assert.
            Assert.IsFalse(result);
            _log.Verify(mock => mock.Error($"{method}: you passed a null matchingKey, the matchingKey must be a non-empty string."), Times.Once());
            _log.Verify(mock => mock.Error($"{method}: you passed a null bucketingKey, the bucketingKey must be a non-empty string."), Times.Once());
        }

        [TestMethod]
        public void IsValid_WhenMatchingKeyAndBucketingKeyAreEmpty_ReturnsFalse()
        {
            // Arrange. 
            var key = new Key(string.Empty, string.Empty);
            var method = "Test";

            // Act.
            var result = keyValidator.IsValid(key, method);

            // Assert.
            Assert.IsFalse(result);
            _log.Verify(mock => mock.Error($"{method}: you passed an empty string, matchingKey must be a non-empty string."), Times.Once());
            _log.Verify(mock => mock.Error($"{method}: you passed an empty string, bucketingKey must be a non-empty string."), Times.Once());
        }

        [TestMethod]
        public void IsValid_WhenMatchingKeyAndBucketingKeyAreLongerThan250Chars_ReturnsFalse()
        {
            // Arrange. 
            var key = new Key("ABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABC1265+5+5+958+54asddasdasdasdCXVXBNVCBV---------////ASDASDSAD3_", null);
            var method = "Test";

            // Act.
            var result = keyValidator.IsValid(key, method);

            // Assert.
            Assert.IsFalse(result);
            _log.Verify(mock => mock.Error($"{method}: matchingKey too long - must be 250 characters or less."), Times.Once());
            _log.Verify(mock => mock.Error($"{method}: bucketingKey too long - must be 250 characters or less."), Times.Once());
        }

        [TestMethod]
        public void IsValid_WhenMatchingKeyAndBucketingKeyOk_ReturnsTrue()
        {
            // Arrange. 
            var key = new Key("ABCABCABCABCABCABCABCABCABCABCABCABC1265+5+5+958+54asddasdasdasdCXVXBNVCBV---------////ASDASDSAD3_", null);
            var method = "Test";

            // Act.
            var result = keyValidator.IsValid(key, method);

            // Assert.
            Assert.IsTrue(result);
        }
    }
}
