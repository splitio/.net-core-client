using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class InMemoryReadinessGatesCacheUnitTests
    {
        [TestMethod]
        public void IsSDKReadyShouldReturnFalseIfSplitsAreNotReady()
        {
            //Arrange
            var gates = new InMemoryReadinessGatesCache();

            //Act
            var result = gates.IsSDKReady(0);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsSDKReadyShouldReturnFalseIfAnySegmentIsNotReady()
        {
            //Arrange
            var gates = new InMemoryReadinessGatesCache();
            gates.RegisterSegment("any");
            gates.SplitsAreReady();

            //Act
            var result = gates.IsSDKReady(0);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsSDKReadyShouldReturnTrueIfSplitsAndSegmentsAreReady()
        {
            //Arrange
            var gates = new InMemoryReadinessGatesCache();
            gates.RegisterSegment("any");
            gates.RegisterSegment("other");
            gates.SplitsAreReady();
            gates.SegmentIsReady("other");
            gates.SegmentIsReady("any");

            //Act
            var result = gates.IsSDKReady(0);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RegisterSegmentShouldReturnFalseIfSplitsAreReady()
        {
            //Arrange
            var gates = new InMemoryReadinessGatesCache();
            gates.SplitsAreReady();

            //Act
            var result = gates.RegisterSegment("any");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RegisterSegmentShouldReturnFalseIfSegmentNameEmpty()
        {
            //Arrange
            var gates = new InMemoryReadinessGatesCache();

            //Act
            var result = gates.RegisterSegment("");

            //Assert
            Assert.IsFalse(result);
        }
    }
}
