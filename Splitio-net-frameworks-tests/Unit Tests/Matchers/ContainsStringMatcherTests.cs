using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Parsing;
using System;
using System.Collections.Generic;

namespace Splitio_net_frameworks_tests.Unit_Tests.Matchers
{
    [TestClass]
    public class ContainsStringMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKeyString()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match("test1");

            //Assert
            Assert.IsTrue(result); //keys contains test1
        }

        [TestMethod]
        public void MatchShouldReturnTrueOnKeyContainingElementString()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match("abctest1abc");

            //Assert
            Assert.IsTrue(result); //keys contains test1
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKeyString()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match("test3");

            //Assert
            Assert.IsFalse(result); //key not contains any element of whitelist
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyWhitelistString()
        {
            //Arrange
            var toCompare = new List<string>();
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match("test1");

            //Assert
            Assert.IsFalse(result); //Empty whitelist
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullKeyString()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            string key = null;
            var result = matcher.Match(key);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyKeyString()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            string key = "";
            var result = matcher.Match(key);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match(new Key("test1", "test1"));

            //Assert
            Assert.IsTrue(result); //keys contains test1
        }

        [TestMethod]
        public void MatchShouldReturnTrueOnKeyContainingElement()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match(new Key("abctest1abc", "abctest1abc"));

            //Assert
            Assert.IsTrue(result); //keys contains test1
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var toCompare = new List<string>();
            toCompare.Add("test1");
            toCompare.Add("test2");
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match(new Key("test3", "test3"));

            //Assert
            Assert.IsFalse(result); //key not contains any element of whitelist
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfEmptyWhitelist()
        {
            //Arrange
            var toCompare = new List<string>();
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match(new Key("test1", "test1"));

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
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            string key = null;
            var result = matcher.Match(new Key(key, key));

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
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            string key = "";
            var result = matcher.Match(new Key(key, key));

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
            var matcher = new ContainsStringMatcher(toCompare);

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
            var matcher = new ContainsStringMatcher(toCompare);

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
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var keys = new List<string>();
            keys.Add("test1");
            var result = matcher.Match(keys);

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
            var matcher = new ContainsStringMatcher(toCompare);

            //Act
            var result = matcher.Match(true);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
