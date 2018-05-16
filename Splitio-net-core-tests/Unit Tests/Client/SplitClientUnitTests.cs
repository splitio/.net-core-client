using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Parsing.Classes;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitClientUnitTests
    {
        private SplitClientForTesting _splitClientForTesting;
        private Mock<ILog> _logMock = new Mock<ILog>();
        private ConcurrentDictionary<string, ParsedSplit> _splits = new ConcurrentDictionary<string, ParsedSplit>();

        [TestInitialize]
        public void TestInitialize()
        {
            // Arrange
            _splitClientForTesting = new SplitClientForTesting(_logMock.Object, _splits);
        }

        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment((string)null, string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullMatchingKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment(new Key(null, string.Empty), string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullMatchingAndBucketingKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment(new Key(null, null), string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullKey()
        {
            // Act
            var result = _splitClientForTesting.Track(null, string.Empty, string.Empty);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullTrafficType()
        {
            // Act
            var result = _splitClientForTesting.Track(string.Empty, null, string.Empty);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullEventType()
        {
            // Act
            var result = _splitClientForTesting.Track(string.Empty, string.Empty, null);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void GetTreatments_WithNullAttributes_ShouldWork()
        {
            // Arrange
            _splits.TryAdd("testSplit", new ParsedSplit
            {
                conditions = new List<ConditionWithLogic>
                {
                    new ConditionWithLogic
                    {
                        conditionType = ConditionType.WHITELIST,
                        label = "test",
                        matcher = new CombiningMatcher
                        {
                            combiner = CombinerEnum.AND,
                            delegates = new List<AttributeMatcher>
                            {
                                new AttributeMatcher { attribute = "testAttribute", matcher = new MatchesStringMatcher(""), negate = false }
                            }
                        }
                    }
                },
                name = "testSplit"                
            });

            // Act
            var result = _splitClientForTesting.GetTreatments("testSplit", new List<string>(), null);

            Assert.IsNotNull(result);
        }
    }
}
