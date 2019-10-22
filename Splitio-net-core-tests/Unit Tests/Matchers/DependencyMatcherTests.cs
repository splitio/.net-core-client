using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Evaluator;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class DependencyMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test", "test");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match(key, null, evaluatorMock.Object);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var treatments = new List<string>() { "off" };
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test", "test");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match(key, null, evaluatorMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullSplitClient()
        {
            //Arrange
            var treatments = new List<string>();
            var matcher = new DependencyMatcher("test1", treatments);
            IEvaluator evaluator = null;

            //Act
            var result = matcher.Match(new Key("test2", "test2"), null, evaluator);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyTreatmentList()
        {
            //Arrange
            var treatments = new List<string>();
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test2", "test2");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match(key, null, evaluatorMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingLong()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test2", "test2");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match(123, null, evaluatorMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingDate()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test2", "test2");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match(DateTime.UtcNow, null, evaluatorMock.Object);

            //Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingList()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test2", "test2");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match(DateTime.UtcNow, null, evaluatorMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingString()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test2", "test2");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match("test", null, evaluatorMock.Object);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingBoolean()
        {
            //Arrange
            var treatments = new List<string>() { "on" };
            var matcher = new DependencyMatcher("test1", treatments);
            var key = new Key("test2", "test2");
            var evaluatorMock = new Mock<IEvaluator>();

            evaluatorMock
                .Setup(mock => mock.Evaluate(key, "test1", null))
                .Returns(new TreatmentResult("label", "on"));

            //Act
            var result = matcher.Match(true, null, evaluatorMock.Object);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
