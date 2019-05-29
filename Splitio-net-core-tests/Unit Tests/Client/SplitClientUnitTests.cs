using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitClientUnitTests
    {        
        private Mock<ILog> _logMock;
        private Mock<IListener<WrappedEvent>> _eventListenerMock;

        private SplitClientForTesting _splitClientForTesting;

        [TestInitialize]
        public void TestInitialize()
        {
            _logMock = new Mock<ILog>();
            _eventListenerMock = new Mock<IListener<WrappedEvent>>();

            _splitClientForTesting = new SplitClientForTesting(_logMock.Object, _eventListenerMock.Object);
        }

        #region GetTreatment
        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment((string)null, string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullMatchingKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment(new Key(null, string.Empty), string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullMatchingAndBucketingKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment(new Key(null, null), string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }
        #endregion

        #region GetTreatmentWithConfig
        [TestMethod]
        public void GetTreatmentWithConfig_WithEmptyKey_ShouldReturnControl()
        {
            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig(string.Empty, string.Empty);

            // Assert
            Assert.AreEqual("control", result.Treatment);
            Assert.IsNull(result.Config);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WithNullKey_ShouldReturnControl()
        {
            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig((Key)null, string.Empty);

            // Assert
            Assert.AreEqual("control", result.Treatment);
            Assert.IsNull(result.Config);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }
        #endregion

        #region GetTreatmentsWithConfig
        [TestMethod]
        public void GetTreatmentsWithConfig_WithEmptyKey_ShouldReturnControl()
        {
            // Act
            var results = _splitClientForTesting.GetTreatmentsWithConfig(string.Empty, new List<string> { string.Empty });

            // Assert
            foreach (var res in results)
            {
                Assert.AreEqual("control", res.Value.Treatment);
                Assert.IsNull(res.Value.Config);
            }

            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_WithNullKey_ShouldReturnControl()
        {
            // Act
            var results = _splitClientForTesting.GetTreatmentsWithConfig((Key)null, new List<string> { string.Empty });

            // Assert
            foreach (var res in results)
            {
                Assert.AreEqual("control", res.Value.Treatment);
                Assert.IsNull(res.Value.Config);
            }

            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }
        #endregion

        #region Track
        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullKey()
        {
            // Act
            var result = _splitClientForTesting.Track(null, string.Empty, string.Empty);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(4));
        }

        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullTrafficType()
        {
            // Act
            var result = _splitClientForTesting.Track(string.Empty, null, string.Empty);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(4));
        }

        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullEventType()
        {
            // Act
            var result = _splitClientForTesting.Track(string.Empty, string.Empty, null);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(4));
        }

        [TestMethod]
        public void Track_WithProperties_RetunrsTrue()
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

            // Act.
            var result = _splitClientForTesting.Track("key", "user", "event_type", 132, properties);

            // Assert.
            Assert.IsTrue(result);
            _logMock.Verify(mock => mock.Warn("Property Splitio.Domain.ParsedSplit is of invalid type. Setting value to null"), Times.Once);
            _eventListenerMock.Verify(mock => mock.Log(It.Is<WrappedEvent>(we => we.Event.properties != null
                                                                              && we.Event.key.Equals("key")
                                                                              && we.Event.eventTypeId.Equals("event_type")
                                                                              && we.Event.trafficTypeName.Equals("user")
                                                                              && we.Event.value == 132)), Times.Once);
        }

        [TestMethod]
        public void Track_WhenPropertiesIsNull_ReturnsTrue()
        {
            // Arrange.
            Dictionary<string, object> properties = null;            

            // Act.
            var result = _splitClientForTesting.Track("key", "user", "event_type", 132, properties);

            // Assert.
            Assert.IsTrue(result);
            _eventListenerMock.Verify(mock => mock.Log(It.Is<WrappedEvent>(we => we.Event.properties == null
                                                                              && we.Event.key.Equals("key")
                                                                              && we.Event.eventTypeId.Equals("event_type")
                                                                              && we.Event.trafficTypeName.Equals("user")
                                                                              && we.Event.value == 132)), Times.Once);
        }
        #endregion
    }
}