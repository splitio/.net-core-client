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
            var result = matcher.Match("test", attributes);

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
            mock1.Setup(x=>x.Match("test",attributes)).Returns(true);
            var mock2 = new Mock<AttributeMatcher>();
            mock2.Setup(x=>x.Match("test",attributes)).Returns(true);
            var mock3 = new Mock<AttributeMatcher>();
            mock3.Setup(x=>x.Match("test",attributes)).Returns(true);

            delegates.Add(mock1.Object);
            delegates.Add(mock2.Object);
            delegates.Add(mock3.Object);

            var matcher = new CombiningMatcher()
            {
                delegates = delegates,
                combiner = CombinerEnum.AND
            };

            //Act
            var result = matcher.Match("test", attributes);

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
            mock1.Setup(x => x.Match("test", attributes)).Returns(true);
            var mock2 = new Mock<AttributeMatcher>();
            mock2.Setup(x => x.Match("test", attributes)).Returns(false);
            var mock3 = new Mock<AttributeMatcher>();
            mock3.Setup(x => x.Match("test", attributes)).Returns(true);

            delegates.Add(mock1.Object);
            delegates.Add(mock2.Object);
            delegates.Add(mock3.Object);

            var matcher = new CombiningMatcher()
            {
                delegates = delegates,
                combiner = CombinerEnum.AND
            };

            //Act
            var result = matcher.Match("test", attributes);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
