using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio_net_frameworks_tests.Unit_Tests.Matchers
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
            var result = matcher.Match(new Key("test", "test"), attributes);

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
            var key = new Key("test", "test");
            var delegates = new List<AttributeMatcher>();
            var mock1 = new Mock<AttributeMatcher>();
            mock1.Setup(x => x.Match(key, attributes, null)).Returns(true);
            var mock2 = new Mock<AttributeMatcher>();
            mock2.Setup(x => x.Match(key, attributes, null)).Returns(true);
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
            mock1.Setup(x => x.Match(new Key("test", "test"), attributes, null)).Returns(true);
            var mock2 = new Mock<AttributeMatcher>();
            mock2.Setup(x => x.Match(new Key("test", "test"), attributes, null)).Returns(false);
            var mock3 = new Mock<AttributeMatcher>();
            mock3.Setup(x => x.Match(new Key("test", "test"), attributes, null)).Returns(true);

            delegates.Add(mock1.Object);
            delegates.Add(mock2.Object);
            delegates.Add(mock3.Object);

            var matcher = new CombiningMatcher()
            {
                delegates = delegates,
                combiner = CombinerEnum.AND
            };

            //Act
            var result = matcher.Match(new Key("test", "test"), attributes);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
