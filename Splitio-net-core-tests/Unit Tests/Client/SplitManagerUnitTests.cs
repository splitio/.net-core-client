using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Client.Classes;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitManagerUnitTests
    {
        private readonly Mock<IBlockUntilReadyService> _blockUntilReadyService;

        private readonly string rootFilePath;

        public SplitManagerUnitTests()
        {
            _blockUntilReadyService = new Mock<IBlockUntilReadyService>();

            // This line is to clean the warnings.
            rootFilePath = string.Empty;

#if NETCORE
            rootFilePath = @"Resources\";
#endif
        }

        [TestMethod]
        public void SplitsReturnSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                conditionType = ConditionType.WHITELIST,
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "off"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);
            var conditionWithLogic2 = new ConditionWithLogic()
            {
                conditionType = ConditionType.ROLLOUT,
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 90, treatment = "on"},
                    new PartitionDefinition(){size = 10, treatment = "off"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic2);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count);
            var firstResult = result.Find(x => x.name == "test1");
            Assert.AreEqual(firstResult.name, "test1");
            Assert.AreEqual(firstResult.changeNumber, 10000);
            Assert.AreEqual(firstResult.killed, false);
            Assert.AreEqual(firstResult.trafficType, "user");
            Assert.AreEqual(firstResult.treatments.Count, 2);
            var firstTreatment = firstResult.treatments[0];
            Assert.AreEqual(firstTreatment, "on");
            var secondTreatment = firstResult.treatments[1];
            Assert.AreEqual(secondTreatment, "off");
        }

        [TestMethod]
        public void SplitsReturnWithNoRolloutConditionSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                conditionType = ConditionType.WHITELIST,
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count);
            var firstResult = result.Find(x => x.name == "test1");
            Assert.AreEqual(firstResult.name, "test1");
            Assert.AreEqual(firstResult.changeNumber, 10000);
            Assert.AreEqual(firstResult.killed, false);
            Assert.AreEqual(firstResult.trafficType, "user");
            Assert.AreEqual(conditionWithLogic.partitions.Count, firstResult.treatments.Count);
        }

        [TestMethod]
        public void SplitReturnSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                conditionType = ConditionType.ROLLOUT,
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 90, treatment = "on"},
                    new PartitionDefinition(){size = 10, treatment = "off"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.name, "test1");
            Assert.AreEqual(result.changeNumber, 10000);
            Assert.AreEqual(result.killed, false);
            Assert.AreEqual(result.trafficType, "user");
            Assert.AreEqual(result.treatments.Count, 2);
            var firstTreatment = result.treatments[0];
            Assert.AreEqual(firstTreatment, "on");
            var secondTreatment = result.treatments[1];
            Assert.AreEqual(secondTreatment, "off");
        }

        [TestMethod]
        public void SplitReturnRolloutConditionTreatmentsSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                conditionType = ConditionType.WHITELIST,
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"},
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var conditionWithLogic2 = new ConditionWithLogic()
            {
                conditionType = ConditionType.ROLLOUT,
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 90, treatment = "on"},
                    new PartitionDefinition(){size = 10, treatment = "off"},
                }
            };
            conditionsWithLogic.Add(conditionWithLogic2);

            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.name, "test1");
            Assert.AreEqual(result.treatments.Count, 2);
            var firstTreatment = result.treatments[0];
            Assert.AreEqual(firstTreatment, "on");
            var secondTreatment = result.treatments[1];
            Assert.AreEqual(secondTreatment, "off");
        }

        [TestMethod]
        public void SplitReturnDefaultTreatmentsWhenNoRolloutCondition()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                conditionType = ConditionType.WHITELIST,
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"},
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.name, "test1");
            Assert.AreEqual(conditionWithLogic.partitions.Count, result.treatments.Count);
        }

        [TestMethod]
        public void SplitReturnsNullWhenInexistent()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitReturnsNullWhenCacheIsNull()
        {
            //Arrange
            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(null, _blockUntilReadyService.Object);
            
            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitsWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SplitsWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Arrange
            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(null, _blockUntilReadyService.Object);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Arrange
            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(null, _blockUntilReadyService.Object);
            
            //Act
            var result = manager.Split("name");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitWithNullNameShouldReturnNull()
        {
            //Arrange
            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);
            
            //Act
            var result = manager.Split(null);

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitNamessWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);
            
            //Act
            var result = manager.SplitNames();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SplitNamessWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Arrange
            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(null, _blockUntilReadyService.Object);
            
            //Act
            var result = manager.SplitNames();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitNamesReturnSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);
            manager.BlockUntilReady(1000);

            //Act
            var result = manager.SplitNames();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count);
            var firstResult = result.Find(x => x == "test1");
            Assert.AreEqual(firstResult, "test1");
        }

        [TestMethod]
        public void Splits_WithConfigs_ReturnSuccessfully()
        {
            //Arrange
            var configurations = new Dictionary<string, string>
            {
                { "On", "\"Name = \"Test Config\"" }
            };

            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic, configurations = configurations });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic, configurations = configurations });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);
            manager.BlockUntilReady(1000);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count);
            var test1Result = result.Find(res => res.name == "test1");
            Assert.IsNotNull(test1Result.configs);
            var test2Result = result.Find(res => res.name == "test2");
            Assert.IsNotNull(test2Result.configs);
            var test3Result = result.Find(res => res.name == "test3");
            Assert.IsNull(test3Result.configs);
        }

        [TestMethod]
        public void Split_WithConfigs_ReturnSuccessfully()
        {
            //Arrange
            var configurations = new Dictionary<string, string>
            {
                { "On", "\"Name = \"Test Config\"" }
            };

            var conditionsWithLogic = new List<ConditionWithLogic>();
            var conditionWithLogic = new ConditionWithLogic()
            {
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            splitCache.AddSplit("test1", new ParsedSplit() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic, configurations = configurations });
            splitCache.AddSplit("test2", new ParsedSplit() { name = "test2", conditions = conditionsWithLogic, configurations = configurations });
            splitCache.AddSplit("test3", new ParsedSplit() { name = "test3", conditions = conditionsWithLogic });
            splitCache.AddSplit("test4", new ParsedSplit() { name = "test4", conditions = conditionsWithLogic });
            splitCache.AddSplit("test5", new ParsedSplit() { name = "test5", conditions = conditionsWithLogic });
            splitCache.AddSplit("test6", new ParsedSplit() { name = "test6", conditions = conditionsWithLogic });

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var manager = new SplitManager(splitCache, _blockUntilReadyService.Object);
            manager.BlockUntilReady(1000);

            //Act
            var result1 = manager.Split("test1");
            var result2 = manager.Split("test2");
            var result3 = manager.Split("test3");

            //Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result1.configs);
            Assert.IsNotNull(result2);
            Assert.IsNotNull(result2.configs);
            Assert.IsNotNull(result3);
            Assert.IsNull(result3.configs);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\split.yaml")]
        public void Split_WithLocalhostClient_WhenNameIsTestingSplitOn_ReturnsSplit()
        {
            // Arrange.
            var splitViewExpected = new SplitView
            {
                name = "testing_split_on",
                treatments = new List<string> { "on" }
            };

            var configurationOptions = new ConfigurationOptions
            {
                LocalhostFilePath = $"{rootFilePath}split.yaml",
                Ready = 500
            };

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var factory = new SplitFactory("localhost", configurationOptions);
            var manager = factory.Manager();
            manager.BlockUntilReady(1000);

            // Act.
            var splitViewResult = manager.Split("testing_split_on");

            // Assert.
            Assert.AreEqual(splitViewExpected.name, splitViewResult.name);
            Assert.IsFalse(splitViewResult.killed);
            Assert.IsNull(splitViewResult.configs);
            Assert.IsNull(splitViewResult.trafficType);
            Assert.AreEqual(splitViewExpected.treatments.Count, splitViewResult.treatments.Count);
            foreach (var treatment in splitViewExpected.treatments)
            {
                Assert.IsNotNull(splitViewResult.treatments.FirstOrDefault(t => t == treatment));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\split.yaml")]
        public void Split_WithLocalhostClient_WhenNameIsTestingSplitOnlyWl_ReturnsSplit()
        {
            // Arrange.
            var splitViewExpected = new SplitView
            {
                name = "testing_split_only_wl",
                treatments = new List<string> { "whitelisted" },
            };

            var configurationOptions = new ConfigurationOptions
            {
                LocalhostFilePath = $"{rootFilePath}split.yaml",
                Ready = 500
            };

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var factory = new SplitFactory("localhost", configurationOptions);
            var manager = factory.Manager();
            manager.BlockUntilReady(1000);

            // Act.
            var splitViewResult = manager.Split("testing_split_only_wl");

            // Assert.
            Assert.AreEqual(splitViewExpected.name, splitViewResult.name);
            Assert.IsFalse(splitViewResult.killed);
            Assert.IsNull(splitViewResult.configs);
            Assert.IsNull(splitViewResult.trafficType);
            Assert.AreEqual(splitViewExpected.treatments.Count, splitViewResult.treatments.Count);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\split.yaml")]
        public void Split_WithLocalhostClient_WhenNameIsTestingSplitWithWl_ReturnsSplit()
        {
            // Arrange.
            var splitViewExpected = new SplitView
            {
                name = "testing_split_with_wl",
                treatments = new List<string> { "not_in_whitelist" },
                configs = new Dictionary<string, string>
                {
                    { "not_in_whitelist", "{\"color\": \"green\"}" },
                    { "multi_key_wl", "{\"color\": \"brown\"}" }
                }
            };

            var configurationOptions = new ConfigurationOptions
            {
                LocalhostFilePath = $"{rootFilePath}split.yaml",
                Ready = 500
            };

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var factory = new SplitFactory("localhost", configurationOptions);
            var manager = factory.Manager();
            manager.BlockUntilReady(1000);

            // Act.
            var splitViewResult = manager.Split("testing_split_with_wl");

            // Assert.
            Assert.AreEqual(splitViewExpected.name, splitViewResult.name);
            Assert.IsFalse(splitViewResult.killed);
            Assert.IsNull(splitViewResult.trafficType);
            Assert.AreEqual(splitViewExpected.configs.Count, splitViewResult.configs.Count);
            foreach (var config in splitViewExpected.configs)
            {
                Assert.AreEqual(config.Value, splitViewResult.configs[config.Key]);
            }

            Assert.AreEqual(splitViewExpected.treatments.Count, splitViewResult.treatments.Count);
            foreach (var treatment in splitViewExpected.treatments)
            {
                Assert.IsNotNull(splitViewResult.treatments.FirstOrDefault(t => t == treatment));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\split.yaml")]
        public void Split_WithLocalhostClient_WhenNameIsTestingSplitOffWithConfig_ReturnsSplit()
        {
            // Arrange.
            var splitViewExpected = new SplitView
            {
                name = "testing_split_off_with_config",
                treatments = new List<string> { "off" },
                configs = new Dictionary<string, string>
                {
                    { "off", "{\"color\": \"green\"}" }
                }
            };

            var configurationOptions = new ConfigurationOptions
            {
                LocalhostFilePath = $"{rootFilePath}split.yaml",
                Ready = 500
            };

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            var factory = new SplitFactory("localhost", configurationOptions);
            var manager = factory.Manager();
            manager.BlockUntilReady(1000);

            // Act.
            var splitViewResult = manager.Split("testing_split_off_with_config");

            // Assert.
            Assert.AreEqual(splitViewExpected.name, splitViewResult.name);
            Assert.IsFalse(splitViewResult.killed);
            Assert.IsNull(splitViewResult.trafficType);
            Assert.AreEqual(splitViewExpected.configs.Count, splitViewResult.configs.Count);
            foreach (var config in splitViewExpected.configs)
            {
                Assert.AreEqual(config.Value, splitViewResult.configs[config.Key]);
            }

            Assert.AreEqual(splitViewExpected.treatments.Count, splitViewResult.treatments.Count);
            foreach (var treatment in splitViewExpected.treatments)
            {
                Assert.IsNotNull(splitViewResult.treatments.FirstOrDefault(t => t == treatment));
            }
        }
    }
}
