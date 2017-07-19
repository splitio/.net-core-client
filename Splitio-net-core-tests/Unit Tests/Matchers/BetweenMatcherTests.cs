using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Parsing;
using Splitio.Domain;
using Splitio.CommonLibraries;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class BetweenMatcherTests
    {
        [TestMethod]
        public void MatchNumberSuccesfully()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.NUMBER, 1000001, 10540001);

            //Act
            var result1 = matcher.Match(1700000);
            var result2 = matcher.Match(545345);
            var result3 = matcher.Match(98981700000);

            //Assert
            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
        }

        [TestMethod]
        public void MatchNumberShouldReturnFalseOnInvalidNumber()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.NUMBER, 1000001, 10540001);

            //Act
            var result = matcher.Match(new Key("1aaaaa0", "1aaaaa0"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchDateSuccesfully()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.DATETIME, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match("1470970000000".ToDateTime().Value);
            var result1 = matcher.Match("1470910000000".ToDateTime().Value);
            var result2 = matcher.Match("1490910000000".ToDateTime().Value);

            //Assert
            Assert.IsTrue(result);
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void MatchDateTruncateToMinutesSuccesfully()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.DATETIME, 1482207323000, 1482207503000);

            //Act
            var date1 = "1482207383000".ToDateTime().Value;
            date1 = date1.AddSeconds(14);
            date1 = date1.AddMilliseconds(324);
            var result = matcher.Match(date1);
            var date2 = "1470916548765".ToDateTime().Value;
            date2 = date2.AddSeconds(12);
            date2 = date2.AddMilliseconds(654);
            var result1 = matcher.Match(date2);
            var date3 = "14909198765443".ToDateTime().Value;
            date3 = date3.AddSeconds(11);
            date3 = date3.AddMilliseconds(456);
            var result2 = matcher.Match(date3);

            //Assert
            Assert.IsTrue(result);
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void MatchDateShouldReturnFalseOnInvalidDate()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.DATETIME, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match(new Key("1aaa0000000", "1aaa0000000"));

            //Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public void MatchShouldReturnFalseOnInvalidDataTypeString()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.STRING, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match(new Key("abcd", "abcd"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnBooleanParameter()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.DATETIME, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match(true);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullOrEmpty()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.DATETIME, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match(new Key("", ""));
            var result2 = matcher.Match(new Key((string)null, null));

            //Assert
            Assert.IsFalse(result);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void MatchNumberShouldReturnFalseOnInvalidNumberWithStringKey()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.NUMBER, 1000001, 10540001);

            //Act
            var result = matcher.Match("1aaaaa0");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchDateShouldReturnFalseOnInvalidDateWithStringKey()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.DATETIME, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match("1aaa0000000");

            //Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public void MatchShouldReturnFalseOnInvalidDataTypeWithStringKey()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.STRING, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match("abcd");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNullOrEmptyWithStringKey()
        {
            //Arrange
            var matcher = new BetweenMatcher(DataTypeEnum.DATETIME, 1470960000000, 1480960000000);

            //Act
            var result = matcher.Match("");
            var result2 = matcher.Match((string)null);

            //Assert
            Assert.IsFalse(result);
            Assert.IsFalse(result2);
        }
    }
}
