using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using System.Collections.Generic;
using Moq;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class CombiningMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnFalseWithNoDelegates()
        {
            //Arrange
            var matcher = new CombiningMatcher()
            {
                delegates = null,
                combiner = CombinerEnum.AND
            };
            
            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");

            //Act
            var key = new Key("test", "test");
            var result = matcher.Match(key, attributes);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnTrueIfAllMatchersMatch()
        {
            //Arrange
            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");

            var delegates = new List<AttributeMatcher>();
            var mock1 = new Mock<AttributeMatcher>();
            var key = new Key("test", "test");
            mock1.Setup(x=>x.Match(key, attributes, null)).Returns(true);
            var mock2 = new Mock<AttributeMatcher>();
            mock2.Setup(x=>x.Match(key, attributes, null)).Returns(true);
            var mock3 = new Mock<AttributeMatcher>();
            mock3.Setup(x=>x.Match(key, attributes, null)).Returns(true);

            delegates.Add(mock1.Object);
            delegates.Add(mock2.Object);
            delegates.Add(mock3.Object);

            var matcher = new CombiningMatcher()
            {
                delegates = delegates,
                combiner = CombinerEnum.AND
            };

            //Act
            var result = matcher.Match(key, attributes);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfAnyMatchersNoMatch()
        {
            //Arrange
            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");

            var delegates = new List<AttributeMatcher>();
            var mock1 = new Mock<AttributeMatcher>();
            var key = new Key("test", "test");
            mock1.Setup(x => x.Match(key, attributes, null)).Returns(true);
            var mock2 = new Mock<AttributeMatcher>();
            mock2.Setup(x => x.Match(key, attributes, null)).Returns(false);
            var mock3 = new Mock<AttributeMatcher>();
            mock3.Setup(x => x.Match(key, attributes, null)).Returns(true);

            delegates.Add(mock1.Object);
            delegates.Add(mock2.Object);
            delegates.Add(mock3.Object);

            var matcher = new CombiningMatcher()
            {
                delegates = delegates,
                combiner = CombinerEnum.AND
            };

            //Act
            var result = matcher.Match(key, attributes);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
