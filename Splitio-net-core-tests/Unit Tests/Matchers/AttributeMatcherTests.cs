using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Parsing;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class AttributeMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueIfAttributeInAttributesIsMatching()
        {
            //Arrange
            var matcher = new AttributeMatcher()
            {
                attribute = "card_number",
                matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 12012),
                negate = false
            };

            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");
            
            //Act
            var result = matcher.Match(null, attributes);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfAttributeInAttributesIsMatchingButResultIsNegated()
        {
            //Arrange
            var matcher = new AttributeMatcher()
            {
                attribute = "card_number",
                matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 12012),
                negate = true
            };

            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");

            //Act
            var result = matcher.Match(null, attributes);

            //Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void MatchShouldReturnFalseIfAttributesDictionaryIsNull()
        {
            //Arrange
            var matcher = new AttributeMatcher()
            {
                attribute = "card_number",
                matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 12012),
                negate = false
            };

            //Act
            var result = matcher.Match(null, null);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfValueForAttributeIsNullAndKeyIsNull()
        {
            //Arrange
            var matcher = new AttributeMatcher()
            {
                attribute = null,
                matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 12012),
                negate = false
            };

            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");

            //Act
            var result = matcher.Match(new Key(null, null), attributes);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfValueForAttributeIsNullAndKeyNotMatching()
        {
            //Arrange
            var matcher = new AttributeMatcher()
            {
                attribute = null,
                matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 12012),
                negate = false
            };

            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");

            //Act
            var result = matcher.Match(new Key("1", "1"), attributes);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnTrueIfValueForAttributeIsNullAndKeyMatching()
        {
            //Arrange
            var matcher = new AttributeMatcher()
            {
                attribute = null,
                matcher = new EqualToMatcher(DataTypeEnum.NUMBER, 12012),
                negate = false
            };

            var attributes = new Dictionary<string, object>();
            attributes.Add("card_number", 12012);
            attributes.Add("card_type", "ABC");

            //Act
            var result = matcher.Match(new Key ("12012", "12012"), attributes);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
