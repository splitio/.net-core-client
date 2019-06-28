using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Client.Classes;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class RedisSplitManagerUnitTests
    {
        private readonly Mock<ISplitCache> _splitCache;
        private readonly Mock<IBlockUntilReadyService> _blockUntilReadyService;
        private RedisSplitManager manager;

        public RedisSplitManagerUnitTests()
        {
            _splitCache = new Mock<ISplitCache>();
            _blockUntilReadyService = new Mock<IBlockUntilReadyService>();

            manager = new RedisSplitManager(_splitCache.Object, _blockUntilReadyService.Object);
        }

        [TestMethod]
        public void SplitsReturnSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionDefinition>();
            var conditionWithLogic = new ConditionDefinition()
            {
                conditionType = "Rollout",
                partitions  = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var splits = new List<SplitBase>
            {
                new Split { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic },
                new Split { name = "test2", conditions = conditionsWithLogic },
                new Split { name = "test3", conditions = conditionsWithLogic },
                new Split { name = "test4", conditions = conditionsWithLogic },
                new Split { name = "test5", conditions = conditionsWithLogic },
                new Split { name = "test6", conditions = conditionsWithLogic }
            };

            _splitCache
                .Setup(x => x.GetAllSplits())
                .Returns(splits);

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count);
            var firstResult = result.Find(x=>x.name == "test1");
            Assert.AreEqual(firstResult.name, "test1");
            Assert.AreEqual(firstResult.changeNumber, 10000);
            Assert.AreEqual(firstResult.killed, false);
            Assert.AreEqual(firstResult.trafficType, "user");
            Assert.AreEqual(firstResult.treatments.Count, 1);
            var firstTreatment = firstResult.treatments[0];
            Assert.AreEqual(firstTreatment, "on");
        }

        [TestMethod]
        public void SplitReturnSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionDefinition>();
            var conditionWithLogic = new ConditionDefinition()
            {
                conditionType = "rollout",
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var split = new Split { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic};

            _splitCache
                .Setup(x => x.GetSplit("test1"))
                .Returns(split);

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.name, "test1");
            Assert.AreEqual(result.changeNumber, 10000);
            Assert.AreEqual(result.killed, false);
            Assert.AreEqual(result.trafficType, "user");
            Assert.AreEqual(result.treatments.Count, 1);
            var firstTreatment = result.treatments[0];
            Assert.AreEqual(firstTreatment, "on");
        }

        [TestMethod]
        public void SplitReturnRolloutConditionTreatmentsSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionDefinition>();
            var conditionWithLogic = new ConditionDefinition()
            {
                conditionType = "whitelist",
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition() { size = 100, treatment = "on"},
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var conditionWithLogic2 = new ConditionDefinition()
            {
                conditionType = "rollout",
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition() { size = 90, treatment = "on"},
                    new PartitionDefinition() { size = 10, treatment = "off"},
                }
            };
            conditionsWithLogic.Add(conditionWithLogic2);

            var split = new Split() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic };

            _splitCache
                .Setup(x => x.GetSplit("test1"))
                .Returns(split);

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

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
            var conditionsWithLogic = new List<ConditionDefinition>();
            var conditionWithLogic = new ConditionDefinition()
            {
                conditionType = "whitelist",
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition() { size = 100, treatment = "on"},
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var split = new Split() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic };

            _splitCache
                .Setup(x => x.GetSplit("test1"))
                .Returns(split);

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

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
            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitReturnsNullWhenCacheIsNull()
        {
            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitsWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            _splitCache
                .Setup(x => x.GetAllSplits())
                .Returns(new List<SplitBase>());

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SplitsWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Act
            var result = manager.Split("name");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitNamessWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            _splitCache
                .Setup(x => x.GetAllSplits())
                .Returns(new List<SplitBase>());

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            //Act
            var result = manager.SplitNames();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SplitNamessWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Act
            var result = manager.SplitNames();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitNamesReturnSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionDefinition>();
            var conditionWithLogic = new ConditionDefinition()
            {
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);

            var splits = new List<SplitBase>
            {
                new Split() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic },
                new Split() { name = "test2", conditions = conditionsWithLogic },
                new Split() { name = "test3", conditions = conditionsWithLogic },
                new Split() { name = "test4", conditions = conditionsWithLogic },
                new Split() { name = "test5", conditions = conditionsWithLogic },
                new Split() { name = "test6", conditions = conditionsWithLogic }
            };

            _splitCache
                .Setup(x => x.GetAllSplits())
                .Returns(splits);

            _blockUntilReadyService
                .Setup(mock => mock.IsSdkReady())
                .Returns(true);

            //Act
            var result = manager.SplitNames();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count);
            var firstResult = result.Find(x => x == "test1");
            Assert.AreEqual(firstResult, "test1");
        }
    }
}
