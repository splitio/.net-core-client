using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;

namespace Splitio_Tests.Unit_Tests.InputValidation
{
    [TestClass]
    public class EventTypeValidatorTests
    {
        private Mock<ISplitLogger> _log;
        private EventTypeValidator eventTypeValidator;

        [TestInitialize]
        public void Initialize()
        {
            _log = new Mock<ISplitLogger>();

            eventTypeValidator = new EventTypeValidator(_log.Object);
        }

        [TestMethod]
        public void IsValid_WhenEventTypeIsEmpty_ReturnsFalse()
        {
            // Arrange. 
            var eventType = string.Empty;
            var method = "Test";

            // Act.
            var result = eventTypeValidator.IsValid(eventType, method);

            // Assert.
            Assert.IsFalse(result);
            _log.Verify(mock => mock.Error($"{method}: you passed an empty event_type, event_type must be a non-empty String"), Times.Once());
        }

        [TestMethod]
        public void IsValid_WhenEventTypeIsNull_ReturnsFalse()
        {
            // Arrange. 
            string eventType = null;
            var method = "Test";

            // Act.
            var result = eventTypeValidator.IsValid(eventType, method);

            // Assert.
            Assert.IsFalse(result);
            _log.Verify(mock => mock.Error($"{method}: you passed a null event_type, event_type must be a non-empty String"), Times.Once());
        }

        [TestMethod]
        public void IsValid_WhenEventTypeHasMoreThan80Characters_ReturnsFalse()
        {
            // Arrange. 
            var eventType = "ABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABCABC123_";
            var method = "Test";
            var Regex = "^[a-zA-Z0-9][-_.:a-zA-Z0-9]{0,79}$";

            // Act.
            var result = eventTypeValidator.IsValid(eventType, method);

            // Assert.
            Assert.IsFalse(result);
            _log.Verify(mock => mock.Error($"{method}: you passed {eventType}, event name must adhere to the regular expression {Regex}. This means an event name must be alphanumeric, cannot be more than 80 characters long, and can only include a dash, underscore, period, or colon as separators of alphanumeric characters"), Times.Once());
        }

        [TestMethod]
        public void IsValid_WhenEventTypeHasWrongCharacter_ReturnsFalse()
        {
            // Arrange. 
            var eventType = "ABC123_*";
            var method = "Test";
            var Regex = "^[a-zA-Z0-9][-_.:a-zA-Z0-9]{0,79}$";

            // Act.
            var result = eventTypeValidator.IsValid(eventType, method);

            // Assert.
            Assert.IsFalse(result);
            _log.Verify(mock => mock.Error($"{method}: you passed {eventType}, event name must adhere to the regular expression {Regex}. This means an event name must be alphanumeric, cannot be more than 80 characters long, and can only include a dash, underscore, period, or colon as separators of alphanumeric characters"), Times.Once());
        }

        [TestMethod]
        public void IsValid_WhenEventTypeIsCorrect_ReturnsTrue()
        {
            // Arrange. 
            var eventType = "ABC123_";
            var method = "Test";

            // Act.
            var result = eventTypeValidator.IsValid(eventType, method);

            // Assert.
            Assert.IsTrue(result);
        }
    }
}
