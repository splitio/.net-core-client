using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Parsing;
using System;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class PartOfSetMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test0");
            toCompare.Add("test1");
            toCompare.Add("test2");
            toCompare.Add("test3");
            toCompare.Add("test4");
            toCompare.Add("test5");

            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test2");
            keys.Add("test3");

            var result = matcher.Match(keys);

            //Assert
            Assert.IsTrue(result); //keys contains test1, test2, test3 contained in whitelist
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnAnyMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test3");

            var result = matcher.Match(keys);

            //Assert
            Assert.IsFalse(result); //keys contains test1 but not test2
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var keys = new List<string>();
            keys.Add("test4");
            keys.Add("test3");

            var result = matcher.Match(keys);

            //Assert
            Assert.IsFalse(result); //keys contains no elements from set
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyWhitelist()
        {
            //Arrange
            var toCompare = new List<string>();
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test3");

            var result = matcher.Match(keys);

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
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            List<string> key = null;
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
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            List<string> key = new List<string>();
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
            var matcher = new PartOfSetMatcher(toCompare);

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
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var result = matcher.Match(DateTime.UtcNow);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var result = matcher.Match(new Key("test", "test"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingString()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var result = matcher.Match("test");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingBoolean()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new PartOfSetMatcher(toCompare);

            //Act
            var result = matcher.Match(true);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
