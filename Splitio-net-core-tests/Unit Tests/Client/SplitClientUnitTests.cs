using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitClientUnitTests
    {
        private SplitClientForTesting _splitClientForTesting;
        private Mock<ILog> _logMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _logMock = new Mock<ILog>();
            _splitClientForTesting = new SplitClientForTesting(_logMock.Object);
        }

        #region GetTreatment
        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment((string)null, string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullMatchingKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment(new Key(null, string.Empty), string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatment_ShouldReturnControl_WithNullMatchingAndBucketingKey()
        {
            // Act
            var result = _splitClientForTesting.GetTreatment(new Key(null, null), string.Empty);

            // Assert
            Assert.AreEqual("control", result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }
        #endregion

        #region GetTreatmentWithConfig
        [TestMethod]
        public void GetTreatmentWithConfig_WithEmptyKey_ShouldReturnControl()
        {
            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig(string.Empty, string.Empty);

            // Assert
            Assert.AreEqual("control", result.Treatment);
            Assert.IsNull(result.Config);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WithNullKey_ShouldReturnControl()
        {
            // Act
            var result = _splitClientForTesting.GetTreatmentWithConfig((Key)null, string.Empty);

            // Assert
            Assert.AreEqual("control", result.Treatment);
            Assert.IsNull(result.Config);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }
        #endregion

        #region GetTreatmentsWithConfig
        [TestMethod]
        public void GetTreatmentsWithConfig_WithEmptyKey_ShouldReturnControl()
        {
            // Act
            var results = _splitClientForTesting.GetTreatmentsWithConfig(string.Empty, new List<string> { string.Empty });

            // Assert
            foreach (var res in results)
            {
                Assert.AreEqual("control", res.Value.Treatment);
                Assert.IsNull(res.Value.Config);
            }

            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_WithNullKey_ShouldReturnControl()
        {
            // Act
            var results = _splitClientForTesting.GetTreatmentsWithConfig((Key)null, new List<string> { string.Empty });

            // Assert
            foreach (var res in results)
            {
                Assert.AreEqual("control", res.Value.Treatment);
                Assert.IsNull(res.Value.Config);
            }

            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(2));
        }
        #endregion

        #region Track
        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullKey()
        {
            // Act
            var result = _splitClientForTesting.Track(null, string.Empty, string.Empty);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(4));
        }

        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullTrafficType()
        {
            // Act
            var result = _splitClientForTesting.Track(string.Empty, null, string.Empty);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(4));
        }

        [TestMethod]
        public void Track_ShouldReturnFalse_WithNullEventType()
        {
            // Act
            var result = _splitClientForTesting.Track(string.Empty, string.Empty, null);

            // Assert
            Assert.IsFalse(result);
            _logMock.Verify(x => x.Error(It.IsAny<string>()), Times.Exactly(4));
        }
        #endregion
    }
}
