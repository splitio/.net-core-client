using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Cache.Classes;
using Splitio.Domain;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Splitio.Services.Client.Classes;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitManagerUnitTests
    {
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

            var manager = new SplitManager(splitCache);

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

            var manager = new SplitManager(splitCache);

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
            Assert.AreEqual(firstResult.treatments.Count, 0);
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

            var manager = new SplitManager(splitCache);

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

            var manager = new SplitManager(splitCache);

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
        public void SplitReturnEmptyTreatmentsWhenNoRolloutCondition()
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

            var manager = new SplitManager(splitCache);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.name, "test1");
            Assert.AreEqual(result.treatments.Count, 0);
        }

        [TestMethod]
        public void SplitReturnsNullWhenInexistent()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());

            var manager = new SplitManager(splitCache);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitReturnsNullWhenCacheIsNull()
        {
            //Arrange
            var manager = new SplitManager(null);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitsWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var manager = new SplitManager(splitCache);

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
            var manager = new SplitManager(null);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Arrange
            var manager = new SplitManager(null);

            //Act
            var result = manager.Split("name");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitWithNullNameShouldReturnNull()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var manager = new SplitManager(splitCache);

            //Act
            var result = manager.Split(null);

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitNamessWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var manager = new SplitManager(splitCache);

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
            var manager = new SplitManager(null);

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

            var manager = new SplitManager(splitCache);

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
            var configurations = new { On = new { Name = "Test Config" } };
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

            var manager = new SplitManager(splitCache);

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
            var configurations = new { On = new { Name = "Test Config" } };
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

            var manager = new SplitManager(splitCache);

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
    }
}
