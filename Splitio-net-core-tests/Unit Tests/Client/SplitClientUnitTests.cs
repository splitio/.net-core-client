using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitClientUnitTests
    {        
        private Mock<ILog> _logMock;
        private Mock<IListener<WrappedEvent>> _eventListenerMock;
        private Mock<ISplitCache> _splitCacheMock;
        private Mock<IListener<KeyImpression>> _impressionListenerMock;
        private Mock<Splitter> _splitterMock;
        private Mock<CombiningMatcher> _combiningMatcher;

        private SplitClientForTesting _splitClientForTesting;

        [TestInitialize]
        public void TestInitialize()
        {
            _logMock = new Mock<ILog>();
            _splitCacheMock = new Mock<ISplitCache>();
            _splitterMock = new Mock<Splitter>();
            _combiningMatcher = new Mock<CombiningMatcher>();
            _eventListenerMock = new Mock<IListener<WrappedEvent>>();
            _impressionListenerMock = new Mock<IListener<KeyImpression>>();

            _splitClientForTesting = new SplitClientForTesting(_logMock.Object, _splitCacheMock.Object, _splitterMock.Object, _eventListenerMock.Object, _impressionListenerMock.Object);
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

        [TestMethod]
        public void GetTreatment_WhenNameDoesntExist_ReturnsControl()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment("key", "not_exist");

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(mock => mock.Warn($"GetTreatment: you passed not_exist that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
            _impressionListenerMock.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Never);
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

        [TestMethod]
        public void GetTreatmentWithConfig_ShouldReturnOnWithConfig()
        {
            // Arrange
            var feature = "always_on";
            var treatmentExpected = "on";
            var configurations = new Dictionary<string, string>
            {
                { "off", "{\"name\": \"off config\", \"lastName\": \"split\"}" },
                { "on", "{\"name\": \"mauro\"}" }
            };

            var parsedSplit = GetParsedSplit(feature, defaultTreatment: "off", configurations: configurations);

            _combiningMatcher
                .Setup(mock => mock.Match(It.IsAny<Key>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<ISplitClient>()))
                .Returns(true);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(feature))
                .Returns(parsedSplit);

            _splitterMock
                .Setup(mock => mock.GetTreatment(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<PartitionDefinition>>(), It.IsAny<AlgorithmEnum>()))
                .Returns(treatmentExpected);

            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("user", feature);

            // Assert
            Assert.AreEqual(treatmentExpected, result.Treatment);
            var configExpected = configurations[treatmentExpected];
            Assert.AreEqual(configExpected, result.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WhenNotMach_ShouldReturnDefaultTreatmentWithConfig()
        {
            // Arrange
            var feature = "always_on";
            var defaultTreatment = "off";
            var configurations = new Dictionary<string, string>
            {
                { "off", "{\"name\": \"off config\", \"lastName\": \"split\"}" },
                { "on", "{\"name\": \"mauro\"}" }
            };

            var parsedSplit = GetParsedSplit(feature, defaultTreatment, configurations: configurations);

            _combiningMatcher
                .Setup(mock => mock.Match(It.IsAny<Key>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<ISplitClient>()))
                .Returns(false);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(feature))
                .Returns(parsedSplit);

            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("user", feature);

            // Assert
            Assert.AreEqual(defaultTreatment, result.Treatment);
            var configExpected = configurations[defaultTreatment];
            Assert.AreEqual(configExpected, result.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WhenConfigIsNull_ShouldReturnOn()
        {
            // Arrange
            var feature = "always_on";
            var defaultTreatment = "off";
            var treatmentExpected = "on";

            var parsedSplit = GetParsedSplit(feature, defaultTreatment);

            _combiningMatcher
                .Setup(mock => mock.Match(It.IsAny<Key>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<ISplitClient>()))
                .Returns(true);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(feature))
                .Returns(parsedSplit);

            _splitterMock
                .Setup(mock => mock.GetTreatment(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<PartitionDefinition>>(), It.IsAny<AlgorithmEnum>()))
                .Returns(treatmentExpected);

            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("user", feature);

            // Assert
            Assert.AreEqual(treatmentExpected, result.Treatment);
            Assert.IsNull(result.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WhenIsKilled_ShouldReturnDefaultTreatmentWithConfig()
        {
            // Arrange
            var feature = "always_on";
            var defaultTreatment = "off";
            var configurations = new Dictionary<string, string>
            {
                { "off", "{\"name\": \"off config\", \"lastName\": \"split\"}" },
                { "on", "{\"name\": \"mauro\"}" }
            };

            var parsedSplit = GetParsedSplit(feature, defaultTreatment, killed: true, configurations: configurations);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(feature))
                .Returns(parsedSplit);

            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("user", feature);

            // Assert
            Assert.AreEqual(defaultTreatment, result.Treatment);
            var configExpected = configurations[defaultTreatment];
            Assert.AreEqual(configExpected, result.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WhenTrafficAllocationIsSmallerThanBucket_ShouldReturnDefaultTreatmentWithConfig()
        {
            // Arrange
            var feature = "always_on";
            var defaultTreatment = "off";
            var configurations = new Dictionary<string, string>
            {
                { "off", "{\"name\": \"off config\", \"lastName\": \"split\"}" },
                { "on", "{\"name\": \"mauro\"}" }
            };

            var parsedSplit = GetParsedSplit(feature, defaultTreatment, configurations: configurations, trafficAllocation: 20);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(feature))
                .Returns(parsedSplit);

            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("user", feature);

            // Assert
            Assert.AreEqual(defaultTreatment, result.Treatment);
            var configExpected = configurations[defaultTreatment];
            Assert.AreEqual(configExpected, result.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WhenConditionTypeIsWhitelist_ShouldReturnOntWithConfig()
        {
            // Arrange
            var feature = "always_on";
            var defaultTreatment = "off";
            var treatmentExpected = "on";
            var configurations = new Dictionary<string, string>
            {
                { "off", "{\"name\": \"off config\", \"lastName\": \"split\"}" },
                { "on", "{\"name\": \"mauro\"}" }
            };

            var conditions = new List<ConditionWithLogic>
            {
                new  ConditionWithLogic
                {
                    conditionType = ConditionType.WHITELIST,
                    label = "default rule",
                    partitions = new List<PartitionDefinition>
                    {
                        new PartitionDefinition
                        {
                            size = 100,
                            treatment = "on"
                        },
                        new PartitionDefinition
                        {
                            size = 0,
                            treatment = "off"
                        }
                    },
                    matcher = _combiningMatcher.Object
                }
            };

            var parsedSplit = GetParsedSplit(feature, defaultTreatment, configurations: configurations, trafficAllocation: 20, conditions: conditions);

            _combiningMatcher
                .Setup(mock => mock.Match(It.IsAny<Key>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<ISplitClient>()))
                .Returns(true);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(feature))
                .Returns(parsedSplit);

            _splitterMock
                .Setup(mock => mock.GetTreatment(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<PartitionDefinition>>(), It.IsAny<AlgorithmEnum>()))
                .Returns(treatmentExpected);

            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("user", feature);

            // Assert
            Assert.AreEqual(treatmentExpected, result.Treatment);
            var configExpected = configurations[treatmentExpected];
            Assert.AreEqual(configExpected, result.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WhenConditionsIsEmpty_ShouldReturnDefaultTreatmenttWithConfig()
        {
            // Arrange
            var feature = "always_on";
            var defaultTreatment = "off";
            var configurations = new Dictionary<string, string>
            {
                { "off", "{\"name\": \"off config\", \"lastName\": \"split\"}" },
                { "on", "{\"name\": \"mauro\"}" }
            };

            var parsedSplit = GetParsedSplit(feature, defaultTreatment, configurations: configurations, trafficAllocation: 20, conditions: new List<ConditionWithLogic>());

            _splitCacheMock
                .Setup(mock => mock.GetSplit(feature))
                .Returns(parsedSplit);

            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("user", feature);

            // Assert
            Assert.AreEqual(defaultTreatment, result.Treatment);
            var configExpected = configurations[defaultTreatment];
            Assert.AreEqual(configExpected, result.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WhenNameDoesntExist_ReturnsControl()
        {
            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig("key", "not_exist");

            // Assert
            Assert.AreEqual("control", result.Treatment);
            Assert.IsNull(result.Config);
            _logMock.Verify(mock => mock.Warn($"GetTreatment: you passed not_exist that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
            _impressionListenerMock.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Never);
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

        [TestMethod]
        public void GetTreatmentsWithConfig_ShouldReturnTreatmentsWithConfigs()
        {
            // Arrange
            var treatmenOn = "always_on";
            var treatmenOff = "always_off";
            var configurations = new Dictionary<string, string>
            {
                { "off", "{\"name\": \"off config\", \"lastName\": \"split\"}" },
                { "on", "{\"name\": \"mauro\"}" }
            };

            var offConditions = new List<ConditionWithLogic>
            {
                new  ConditionWithLogic
                {
                    conditionType = ConditionType.ROLLOUT,
                    label = "default rule",
                    partitions = new List<PartitionDefinition>
                    {
                        new PartitionDefinition
                        {
                            size = 100,
                            treatment = "off"
                        },
                        new PartitionDefinition
                        {
                            size = 0,
                            treatment = "on"
                        }
                    },
                    matcher = _combiningMatcher.Object
                }
            };

            var parsedSplitOn = GetParsedSplit(treatmenOn, defaultTreatment: "off", configurations: configurations);
            var parsedSplitOff = GetParsedSplit(treatmenOff, defaultTreatment: "on", configurations: configurations, conditions: offConditions, seed: 2095087413);

            _combiningMatcher
                .Setup(mock => mock.Match(It.IsAny<Key>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<ISplitClient>()))
                .Returns(true);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(treatmenOn))
                .Returns(parsedSplitOn);

            _splitCacheMock
                .Setup(mock => mock.GetSplit(treatmenOff))
                .Returns(parsedSplitOff);

            _splitterMock
                .Setup(mock => mock.GetTreatment(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<PartitionDefinition>>(), It.IsAny<AlgorithmEnum>()))
                .Returns("on");

            _splitterMock
                .Setup(mock => mock.GetTreatment("user", 2095087413, It.IsAny<List<PartitionDefinition>>(), AlgorithmEnum.Murmur))
                .Returns("off");

            // Act
            var result = _splitClientForTesting.GetTreatmentsWithConfig("user", new List<string> { treatmenOff, treatmenOn });

            // Assert
            var resultOn = result[parsedSplitOn.name];
            Assert.AreEqual("on", resultOn.Treatment);
            var configExpected = configurations[resultOn.Treatment];
            Assert.AreEqual(configExpected, resultOn.Config);

            var resultOff = result[parsedSplitOff.name];
            Assert.AreEqual("off", resultOff.Treatment);
            configExpected = configurations[resultOff.Treatment];
            Assert.AreEqual(configExpected, resultOff.Config);
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_WhenNameDoesntExist_ReturnsControl()
        {
            // Arrange 
            var splitNames = new List<string> { "not_exist" , "not_exist2" };

            // Act
            var result = _splitClientForTesting.GetTreatmentsWithConfig("key", splitNames);

            // Assert
            foreach (var res in result)
            {
                Assert.AreEqual("control", res.Value.Treatment);
                Assert.IsNull(res.Value.Config);
                _logMock.Verify(mock => mock.Warn($"GetTreatment: you passed {res.Key} that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
            }

            _impressionListenerMock.Verify(mock => mock.Log(It.IsAny<KeyImpression>()), Times.Never);
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

        [TestMethod]
        public void Track_WhenTraffictTypeDoesNotExist_ReturnsTrue()
        {
            // Arrange.
            var trafficType = "traffict_type";

            _splitCacheMock
                .Setup(mock => mock.TrafficTypeExists(trafficType))
                .Returns(false);

            // Act.
            var result = _splitClientForTesting.Track("key", trafficType, "event_type", 132);

            // Assert.
            Assert.IsTrue(result);
            _eventListenerMock.Verify(mock => mock.Log(It.Is<WrappedEvent>(we => we.Event.properties == null
                                                                              && we.Event.key.Equals("key")
                                                                              && we.Event.eventTypeId.Equals("event_type")
                                                                              && we.Event.trafficTypeName.Equals(trafficType)
                                                                              && we.Event.value == 132)), Times.Once);

            _logMock.Verify(mock => mock.Warn($"Track: Traffic Type {trafficType} does not have any corresponding Splits in this environment, make sure you’re tracking your events to a valid traffic type defined in the Split console."), Times.Once);
        }

        [TestMethod]
        public void Track_WhenTraffictTypeExists_ReturnsTrue()
        {
            // Arrange.
            var trafficType = "traffict_type";

            _splitCacheMock
                .Setup(mock => mock.TrafficTypeExists(trafficType))
                .Returns(true);

            // Act.
            var result = _splitClientForTesting.Track("key", trafficType, "event_type", 132);

            // Assert.
            Assert.IsTrue(result);
            _eventListenerMock.Verify(mock => mock.Log(It.Is<WrappedEvent>(we => we.Event.properties == null
                                                                              && we.Event.key.Equals("key")
                                                                              && we.Event.eventTypeId.Equals("event_type")
                                                                              && we.Event.trafficTypeName.Equals(trafficType)
                                                                              && we.Event.value == 132)), Times.Once);

            _logMock.Verify(mock => mock.Warn($"Track: Traffic Type {trafficType} does not have any corresponding Splits in this environment, make sure you’re tracking your events to a valid traffic type defined in the Split console."), Times.Never);
        }
        #endregion

        #region Private Methods
        private ParsedSplit GetParsedSplit(string name, string defaultTreatment, bool killed = false, Dictionary<string, string> configurations = null, List<ConditionWithLogic> conditions = null, int? trafficAllocation = null, int? seed = null)
        {
            return new ParsedSplit
            {
                algo = AlgorithmEnum.Murmur,
                changeNumber = 1556063594549,
                defaultTreatment = defaultTreatment,
                killed = killed,
                name = name,
                seed = seed ?? 2095087412,
                trafficAllocation = trafficAllocation ?? 100,
                trafficAllocationSeed = -1953939473,
                trafficTypeName = "user",
                configurations = configurations,
                conditions = conditions ?? new List<ConditionWithLogic>
                {
                    new  ConditionWithLogic
                    {
                        conditionType = ConditionType.ROLLOUT,
                        label = "default rule",
                        partitions = new List<PartitionDefinition>
                        {
                            new PartitionDefinition
                            {
                                size = 100,
                                treatment = "on"
                            },
                            new PartitionDefinition
                            {
                                size = 0,
                                treatment = "off"
                            }
                        },
                        matcher = _combiningMatcher.Object
                    }
                }
            };
        }
        #endregion
    }
}