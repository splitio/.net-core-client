using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using System.Collections.Generic;
using Splitio.Services.Client.Classes;
using Moq;
using Splitio.Services.Cache.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class RedisSplitManagerUnitTests
    {
        [TestMethod]
        public void SplitsReturnSuccessfully()
        {
            //Arrange
            var conditionsWithLogic = new List<ConditionDefinition>();
            var conditionWithLogic = new ConditionDefinition()
            {
                partitions  = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);
            var splitCache = new Mock<ISplitCache>();
            var splits = new List<SplitBase>();
            splits.Add(new Split() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic});
            splits.Add(new Split() { name = "test2", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test3", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test4", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test5", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test6", conditions = conditionsWithLogic });
            splitCache.Setup(x => x.GetAllSplits()).Returns(splits);
            

            var manager = new RedisSplitManager(splitCache.Object);
            
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
                partitions = new List<PartitionDefinition>()
                {
                    new PartitionDefinition(){size = 100, treatment = "on"}
                }
            };
            conditionsWithLogic.Add(conditionWithLogic);
            var splitCache = new Mock<ISplitCache>();
            var split = new Split() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic};

            splitCache.Setup(x => x.GetSplit("test1")).Returns(split);

            var manager = new RedisSplitManager(splitCache.Object);

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
        public void SplitReturnsNullWhenInexistent()
        {
            //Arrange
            var splitCache = new Mock<ISplitCache>();
            var manager = new RedisSplitManager(splitCache.Object);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitReturnsNullWhenCacheIsNull()
        {
            //Arrange
            var manager = new RedisSplitManager(null);

            //Act
            var result = manager.Split("test1");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitsWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            var splitCache = new Mock<ISplitCache>();
            splitCache.Setup(x => x.GetAllSplits()).Returns(new List<SplitBase>());
            var manager = new RedisSplitManager(splitCache.Object);

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
            var manager = new RedisSplitManager(null);

            //Act
            var result = manager.Splits();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitWhenCacheIsNotInstancedShouldReturnNull()
        {
            //Arrange
            var manager = new RedisSplitManager(null);

            //Act
            var result = manager.Split("name");

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SplitNamessWhenCacheIsEmptyShouldReturnEmptyList()
        {
            //Arrange
            var splitCache = new Mock<ISplitCache>();
            splitCache.Setup(x => x.GetAllSplits()).Returns(new List<SplitBase>());
            var manager = new RedisSplitManager(splitCache.Object);

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
            var manager = new RedisSplitManager(null);

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
            var splitCache = new Mock<ISplitCache>();
            var splits = new List<SplitBase>();
            splits.Add(new Split() { name = "test1", changeNumber = 10000, killed = false, trafficTypeName = "user", seed = -1, conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test2", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test3", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test4", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test5", conditions = conditionsWithLogic });
            splits.Add(new Split() { name = "test6", conditions = conditionsWithLogic });
            splitCache.Setup(x => x.GetAllSplits()).Returns(splits);
            
            var manager = new RedisSplitManager(splitCache.Object);

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
