using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Parsing;
using System;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class StartsWithMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            var result = matcher.Match("test1end");

            //Assert
            Assert.IsTrue(result); //test1end starts with test1
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            var result = matcher.Match("test3end");

            //Assert
            Assert.IsFalse(result); //key not starts with any element of whitelist
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyWhitelist()
        {
            //Arrange
            var toCompare = new List<string>();
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            var result = matcher.Match("test1");

            //Assert
            Assert.IsFalse(result); //Empty whitelist
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            string key = null;
            var result = matcher.Match(key);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            string key = "";
            var result = matcher.Match(key);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingLong()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            var result = matcher.Match(123);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingDate()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            var result = matcher.Match(DateTime.UtcNow);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingSet()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new StartsWithMatcher(toCompare);

            //Act
            var keys = new List<string>();
            keys.Add("test1");
            var result = matcher.Match(keys);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
