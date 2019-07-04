using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.InputValidation.Classes;
using System.Collections.Generic;
using System.Linq;

namespace Splitio_net_frameworks_tests.Unit_Tests.InputValidation
{
    [TestClass]
    public class SplitNameValidatorTests
    {
        private Mock<ILog> _log;
        private SplitNameValidator splitNameValidator;

        [TestInitialize]
        public void Initialize()
        {
            _log = new Mock<ILog>();

            splitNameValidator = new SplitNameValidator(_log.Object);
        }

        [TestMethod]
        public void SplitNameIsValid_WhenSplitNameIsEmpty_ReturnsFalse()
        {
            // Arrange.
            var splitName = string.Empty;
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNameIsValid(splitName, method);

            // Assert.
            Assert.IsFalse(result.Success);
            _log.Verify(mock => mock.Error($"{method}: you passed an empty split_name, split_name must be a non-empty string"), Times.Once());
        }

        [TestMethod]
        public void SplitNameIsValid_WhenSplitNameIsNull_ReturnsFalse()
        {
            // Arrange.
            string splitName = null;
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNameIsValid(splitName, method);

            // Assert.
            Assert.IsFalse(result.Success);
            _log.Verify(mock => mock.Error($"{method}: you passed a null split_name, split_name must be a non-empty string"), Times.Once());
        }

        [TestMethod]
        public void SplitNameIsValid_WhenSplitNameHasSpacewhite_ReturnsTrue()
        {
            // Arrange.
            var splitName = "  ASD F654   ";
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNameIsValid(splitName, method);

            // Assert.
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(splitName, result.Value);
            Assert.AreEqual("ASD F654", result.Value);
            _log.Verify(mock => mock.Warn($"{method}: split name {splitName} has extra whitespace, trimming"), Times.Once());
        }

        [TestMethod]
        public void SplitNameIsValid_WhenSplitNameIsOk_ReturnsTrue()
        {
            // Arrange.
            var splitName = "ASD F654";
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNameIsValid(splitName, method);

            // Assert.
            Assert.IsTrue(result.Success);
            Assert.AreEqual(splitName, result.Value);
        }

        [TestMethod]
        public void SplitNamesIsValid_WhenSplitNamesIsNull_ReturnsNull()
        {
            // Arrange.
            List<string> splitNames = null;
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNamesAreValid(splitNames, method);
            
            // Assert.
            Assert.IsNull(result);
            _log.Verify(mock => mock.Error($"{method}: split_names must be a non-empty array"));
        }

        [TestMethod]
        public void SplitNamesIsValid_WhenSplitNamesIsEmpty_ReturnsEmptyList()
        {
            // Arrange.
            var splitNames = new List<string>();
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNamesAreValid(splitNames, method);

            // Assert.
            Assert.IsFalse(result.Any());
            _log.Verify(mock => mock.Error($"{method}: split_names must be a non-empty array"));
        }

        [TestMethod]
        public void SplitNamesIsValid_WhenSplitNamesHasValue_ReturnsSplitNames()
        {
            // Arrange.
            var splitNames = new List<string>
            {
                "split_name_1",
                "split_name_2",
                "split_name_3"
            };
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNamesAreValid(splitNames, method);

            // Assert.
            Assert.AreEqual(splitNames.Count, result.Count);
        }

        [TestMethod]
        public void SplitNamesIsValid_WhenHaveRepeatedNames_ReturnsSplitNames()
        {
            // Arrange.
            var splitNames = new List<string>
            {
                "split_name_1",
                "split_name_2",
                "split_name_3",
                "split_name_2",
            };
            var method = "Test";

            // Act.
            var result = splitNameValidator.SplitNamesAreValid(splitNames, method);

            // Assert.
            Assert.AreNotEqual(splitNames.Count, result.Count);
            Assert.AreEqual(3, result.Count);
        }
    }
}
