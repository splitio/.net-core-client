using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Parsing;
using System;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class WhitelistMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test2");
            var matcher = new WhitelistMatcher(keys);

            //Act
            var result = matcher.Match("test2");

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test2");
            var matcher = new WhitelistMatcher(keys);

            //Act
            var result = matcher.Match("test3");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyWhitelist()
        {
            //Arrange
            var keys = new List<string>();
            var matcher = new WhitelistMatcher(keys);

            //Act
            var result = matcher.Match("test2");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingLong()
        {
            //Arrange
            var keys = new List<string>();
            var matcher = new WhitelistMatcher(keys);

            //Act
            var result = matcher.Match(123);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingDate()
        {
            //Arrange
            var keys = new List<string>();
            var matcher = new WhitelistMatcher(keys);

            //Act
            var result = matcher.Match(DateTime.UtcNow);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
