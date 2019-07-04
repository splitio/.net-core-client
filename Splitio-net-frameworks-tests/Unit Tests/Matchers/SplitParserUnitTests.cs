using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Parsing.Classes;
using System.Collections.Generic;

namespace Splitio_net_frameworks_tests.Unit_Tests.Matchers
{
    [TestClass]
    public class SplitParserUnitTests
    {
        [TestMethod]
        public void ParseSuccessfullyWhenNonSpecifiedAlgorithm()
        {
            //Arrange
            Split split = new Split();
            split.name = "test1";
            split.seed = 2323;
            split.status = "ACTIVE";
            split.killed = false;
            split.defaultTreatment = "off";
            split.changeNumber = 232323;
            split.trafficTypeName = "user";
            split.conditions = new List<ConditionDefinition>();

            var parser = new InMemorySplitParser(null, null);

            //Act
            var parsedSplit = parser.Parse(split);

            //Assert
            Assert.IsNotNull(parsedSplit);
            Assert.AreEqual(split.name, parsedSplit.name);
            Assert.AreEqual(split.seed, parsedSplit.seed);
            Assert.AreEqual(split.killed, parsedSplit.killed);
            Assert.AreEqual(split.defaultTreatment, parsedSplit.defaultTreatment);
            Assert.AreEqual(split.changeNumber, parsedSplit.changeNumber);
            Assert.AreEqual(AlgorithmEnum.LegacyHash, parsedSplit.algo);
            Assert.AreEqual(split.trafficTypeName, parsedSplit.trafficTypeName);
        }

        [TestMethod]
        public void ParseSuccessfullyWhenLegacyAlgorithm()
        {
            //Arrange
            Split split = new Split();
            split.name = "test1";
            split.seed = 2323;
            split.status = "ACTIVE";
            split.killed = false;
            split.defaultTreatment = "off";
            split.changeNumber = 232323;
            split.algo = 1;
            split.trafficTypeName = "user";
            split.conditions = new List<ConditionDefinition>();

            var parser = new InMemorySplitParser(null, null);

            //Act
            var parsedSplit = parser.Parse(split);

            //Assert
            Assert.IsNotNull(parsedSplit);
            Assert.AreEqual(split.name, parsedSplit.name);
            Assert.AreEqual(split.seed, parsedSplit.seed);
            Assert.AreEqual(split.killed, parsedSplit.killed);
            Assert.AreEqual(split.defaultTreatment, parsedSplit.defaultTreatment);
            Assert.AreEqual(split.changeNumber, parsedSplit.changeNumber);
            Assert.AreEqual(AlgorithmEnum.LegacyHash, parsedSplit.algo);
            Assert.AreEqual(split.trafficTypeName, parsedSplit.trafficTypeName);
        }

        [TestMethod]
        public void ParseSuccessfullyWhenMurmurAlgorithm()
        {
            //Arrange
            Split split = new Split();
            split.name = "test1";
            split.seed = 2323;
            split.status = "ACTIVE";
            split.killed = false;
            split.defaultTreatment = "off";
            split.changeNumber = 232323;
            split.algo = 2;
            split.trafficTypeName = "user";
            split.conditions = new List<ConditionDefinition>();

            var parser = new InMemorySplitParser(null, null);

            //Act
            var parsedSplit = parser.Parse(split);

            //Assert
            Assert.IsNotNull(parsedSplit);
            Assert.AreEqual(split.name, parsedSplit.name);
            Assert.AreEqual(split.seed, parsedSplit.seed);
            Assert.AreEqual(split.killed, parsedSplit.killed);
            Assert.AreEqual(split.defaultTreatment, parsedSplit.defaultTreatment);
            Assert.AreEqual(split.changeNumber, parsedSplit.changeNumber);
            Assert.AreEqual(AlgorithmEnum.Murmur, parsedSplit.algo);
            Assert.AreEqual(split.trafficTypeName, parsedSplit.trafficTypeName);
        }
    }
}
