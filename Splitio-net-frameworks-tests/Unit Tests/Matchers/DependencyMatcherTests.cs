using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;

namespace Splitio_net_frameworks_tests.Unit_Tests.Matchers
{
    [TestClass]
    public class DependencyMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var treatments = new List<string>(){"on"};
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            var key = new Key("test", "test");
            splitClientMock.Setup(x => x.GetTreatment(key, "test1", null, false, false)).Returns("on");
            //Act
            var result = matcher.Match(key, null, splitClientMock.Object);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var treatments = new List<string>() { "off" };
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            splitClientMock.Setup(x => x.GetTreatment("test", "test1", null, false, false)).Returns("on");
            //Act
            var result = matcher.Match(new Key("test", "test"), null, splitClientMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullSplitClient()
        {
            //Arrange
            var treatments = new List<string>();
            var matcher = new DependencyMatcher("test1", treatments);
            ISplitClient splitClient = null;

            //Act
            var result = matcher.Match(new Key("test2", "test2"), null, splitClient);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyTreatmentList()
        {
            //Arrange
            var treatments = new List<string>();
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            splitClientMock.Setup(x => x.GetTreatment("test", "test1", null, false, false)).Returns("on");

            //Act
            var result = matcher.Match(new Key("test2", "test2"), null, splitClientMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingLong()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            splitClientMock.Setup(x => x.GetTreatment("test", "test1", null, false, false)).Returns("on");

            //Act
            var result = matcher.Match(123, null, splitClientMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingDate()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            splitClientMock.Setup(x => x.GetTreatment("test", "test1", null, false, false)).Returns("on");

            //Act
            var result = matcher.Match(DateTime.UtcNow, null, splitClientMock.Object);

            //Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingList()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            splitClientMock.Setup(x => x.GetTreatment("test", "test1", null, false, false)).Returns("on");

            //Act
            var result = matcher.Match(DateTime.UtcNow, null, splitClientMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingString()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            splitClientMock.Setup(x => x.GetTreatment("test", "test1", null, false, false)).Returns("on");

            //Act
            var result = matcher.Match("test", null, splitClientMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingBoolean()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var splitClientMock = new Mock<ISplitClient>();
            splitClientMock.Setup(x => x.GetTreatment("test", "test1", null, false, false)).Returns("on");

            //Act
            var result = matcher.Match(true, null, splitClientMock.Object);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
