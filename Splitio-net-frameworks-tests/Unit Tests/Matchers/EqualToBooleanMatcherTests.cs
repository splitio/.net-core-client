using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;

namespace Splitio_net_frameworks_tests.Unit_Tests.Matchers
{
    [TestClass]
    public class EqualToBooleanMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);
            var matcher2 = new EqualToBooleanMatcher(false);

            //Act
            var result = matcher.Match(true);
            var result2 = matcher2.Match(false);

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);
            var matcher2 = new EqualToBooleanMatcher(false);

            //Act
            var result = matcher.Match(false);
            var result2 = matcher2.Match(true);

            //Assert
            Assert.IsFalse(result);
            Assert.IsFalse(result2);

        }


        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingLong()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);

            //Act
            var result = matcher.Match(123);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingDate()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);

            //Act
            var result = matcher.Match(DateTime.UtcNow);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingKey()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);

            //Act
            var result = matcher.Match(new Key("test","test"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingStringNotBoolean()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);

            //Act
            var result = matcher.Match("testring");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnTrueIfMatchingStringBoolean()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);

            //Act
            var result = matcher.Match("true");

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingSet()
        {
            //Arrange
            var matcher = new EqualToBooleanMatcher(true);

            //Act
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test3");

            var result = matcher.Match(keys);

            //Assert
            Assert.IsFalse(result);
        }

    }
}
