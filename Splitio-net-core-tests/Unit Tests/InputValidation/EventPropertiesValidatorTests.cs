using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.InputValidation.Classes;
using Splitio_Tests.Resources;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.InputValidation
{
    [TestClass]
    public class EventPropertiesValidatorTests
    {
        private Mock<ILog> _log;
        private EventPropertiesValidator eventPropertiesValidator;

        [TestInitialize]
        public void Initialize()
        {
            _log = new Mock<ILog>();

            eventPropertiesValidator = new EventPropertiesValidator(_log.Object);
        }

        [TestMethod]
        public void IsValid_WhenPropertiesIsNull_ReturnsTrue()
        {
            // Act.
            var result = eventPropertiesValidator.IsValid(properties: null);

            // Assert.
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Value);
            Assert.IsTrue(result.EventSize == default(long));
        }

        [TestMethod]
        public void IsValid_WhenPropertiesIsNotNull_ReturnsTrue()
        {
            // Arrange. 
            decimal decimalValue = 111;
            float floatValue = 112;
            double doubleValue = 113;
            short shortValue = 114;
            int intValue = 115;
            long longValue = 116;
            ushort ushortValue = 117;
            uint uintValue = 118;
            ulong ulongValue = 119;

            var properties = new Dictionary<string, object>
            {
                { "property_1", "value1" },
                { "property_2", new ParsedSplit() },
                { "property_3", false },
                { "property_4", null },
                { "property_5", decimalValue },
                { "property_6", floatValue },
                { "property_7", doubleValue },
                { "property_8", shortValue },
                { "property_9", intValue },
                { "property_10", longValue },
                { "property_11", ushortValue },
                { "property_12", uintValue },
                { "property_13", ulongValue }
            };

            var sizeExpected = 1024L;
            foreach (var item in properties)
            {
                sizeExpected += item.Key.Length;

                if (item.Value is string)
                    sizeExpected += ((string)item.Value).Length;
            }

            // Act.
            var result = eventPropertiesValidator.IsValid(properties);

            // Assert.
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);

            var dicResult = (Dictionary<string, object>)result.Value;
            Assert.AreEqual("value1", dicResult["property_1"]);
            Assert.IsNull(dicResult["property_2"]);
            Assert.IsFalse((bool)dicResult["property_3"]);
            Assert.IsNull(dicResult["property_4"]);
            Assert.AreEqual(decimalValue, dicResult["property_5"]);
            Assert.AreEqual(floatValue, dicResult["property_6"]);
            Assert.AreEqual(doubleValue, dicResult["property_7"]);
            Assert.AreEqual(shortValue, dicResult["property_8"]);
            Assert.AreEqual(intValue, dicResult["property_9"]);
            Assert.AreEqual(longValue, dicResult["property_10"]);
            Assert.AreEqual(ushortValue, dicResult["property_11"]);
            Assert.AreEqual(uintValue, dicResult["property_12"]);
            Assert.AreEqual(ulongValue, dicResult["property_13"]);
            Assert.IsTrue(result.EventSize == sizeExpected);

            _log.Verify(mock => mock.Warn($"Property Splitio.Domain.ParsedSplit is of invalid type. Setting value to null"), Times.Once);
            _log.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Exactly(1));
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void IsValid_WhenPropertiesCountIsBiggerThan300_ReturnsTrue()
        {
            // Arrange. 
            var properties = new Dictionary<string, object>();

            for (int i = 0; i < 400; i++)
            {
                properties.Add($"property_{i}", $"value_{i}");
            }

            var sizeExpected = 1024L;
            foreach (var item in properties)
            {
                sizeExpected += item.Key.Length;

                if (item.Value is string)
                    sizeExpected += ((string)item.Value).Length;
            }

            // Act.
            var result = eventPropertiesValidator.IsValid(properties);

            // Assert.
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.EventSize == sizeExpected);

            _log.Verify(mock => mock.Warn("Event has more than 300 properties. Some of them will be trimmed when processed"), Times.Once);
            _log.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Exactly(1));
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void IsValid_WhenPropertiesBytesIsBiggerThanWeSupport_ReturnsFalse()
        {
            // Arrange. 
            var properties = new Dictionary<string, object>();

            for (int i = 0; i < 400; i++)
            {
                properties.Add($"property_{i}", SplitsHelper.RandomText);
            }

            // Act.
            var result = eventPropertiesValidator.IsValid(properties);

            // Assert.
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Value);
            Assert.IsTrue(result.EventSize == default(long));

            _log.Verify(mock => mock.Warn("Event has more than 300 properties. Some of them will be trimmed when processed"), Times.Once);
            _log.Verify(mock => mock.Warn(It.IsAny<string>()), Times.Exactly(1));
            _log.Verify(mock => mock.Error($"The maximum size allowed for the properties is 32768 bytes. Current one is 33376 bytes. Event not queued"), Times.Once);
            _log.Verify(mock => mock.Error(It.IsAny<string>()), Times.Exactly(1));
        }
    }
}
