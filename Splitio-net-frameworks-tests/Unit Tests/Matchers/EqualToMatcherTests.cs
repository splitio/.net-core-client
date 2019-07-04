using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Parsing;
using Splitio.Domain;
using Splitio.CommonLibraries;
using System;

namespace Splitio_net_frameworks_tests.Unit_Tests.Matchers
{
    [TestClass]
    public class EqualToMatcherTests
    {
        [TestMethod]
        public void MatchNumberSuccesfully()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 1000001);

            //Act
            var result1 = matcher.Match(1000001);
            var result2 = matcher.Match(545345);

            //Assert
            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void MatchNumberShouldReturnFalseOnKey()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 1000001);

            //Act
            var result = matcher.Match(new Key("1aaaaa0", "1aaaaa0"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchDateSuccesfully()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.DATETIME, 1470960000000);

            //Act
            var result = matcher.Match("1470960000000".ToDateTime().Value);
            var result1 = matcher.Match("1470910000000".ToDateTime().Value);

            //Assert
            Assert.IsTrue(result);
            Assert.IsFalse(result1);
        }

        [TestMethod]
        public void MatchDateTruncateToDaySuccesfully()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.DATETIME, 1470960000000);

            //Act
            var date1 = "1470960000000".ToDateTime().Value;
            date1 = date1.AddSeconds(14);
            date1 = date1.AddMilliseconds(324);
            var result = matcher.Match(date1);
            var date2 = "1470910000000".ToDateTime().Value;
            date2 = date2.AddSeconds(12);
            date2 = date2.AddMilliseconds(654);
            var result1 = matcher.Match(date2);

            //Assert
            Assert.IsTrue(result);
            Assert.IsFalse(result1);
        }

        [TestMethod]
        public void MatchDateShouldReturnFalseOnInvalidDateKey()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.DATETIME, 1470960000000);

            //Act
            var result = matcher.Match(new Key("1aaa0000000", "1aaa0000000"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnInvalidDataTypeKey()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.STRING, 1470960000000);

            //Act
            var result = matcher.Match(new Key("abcd", "abcd"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullOrEmptyKey()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.DATETIME, 1470960000000);

            //Act
            var result = matcher.Match(new Key("", ""));
            var result2 = matcher.Match(new Key((String)null, (String)null));

            //Assert
            Assert.IsFalse(result);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void MatchNumberShouldReturnFalseOnInvalidNumber()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 1000001);

            //Act
            var result = matcher.Match("1aaaaa0");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchDateShouldReturnFalseOnInvalidDate()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.DATETIME, 1470960000000);

            //Act
            var result = matcher.Match("1aaa0000000");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnInvalidDataTypeString()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.STRING, 1470960000000);

            //Act
            var result = matcher.Match("abcd");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnBooleanParameter()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.DATETIME, 1470960000000);

            //Act
            var result = matcher.Match(true);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullOrEmpty()
        {
            //Arrange
            var matcher = new EqualToMatcher(DataTypeEnum.DATETIME, 1470960000000);

            //Act
            var result = matcher.Match("");
            var result2 = matcher.Match((String)null);

            //Assert
            Assert.IsFalse(result);
            Assert.IsFalse(result2);
        }
    }
}
