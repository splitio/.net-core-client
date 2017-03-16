using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.CommonLibraries;

namespace Splitio_Tests.Unit_Tests.CommonLibraries
{
    [TestClass]
    public class TypeConverterUnitTests
    {
        [TestMethod]
        public void ConvertTimeStampToDateTime()
        {
            //Arrange
            long value = 1475594054551;

            //Act
            var result = value.ToDateTime();

            //Assert
            Assert.AreEqual(4, result.Day);
            Assert.AreEqual(10, result.Month);
            Assert.AreEqual(2016, result.Year);
            Assert.AreEqual(15, result.Hour);
            Assert.AreEqual(14, result.Minute);
            Assert.AreEqual(0, result.Second);
            Assert.AreEqual(0, result.Millisecond);
        }


        [TestMethod]
        public void ConvertTimeStampStringToDateTimeSucessfully()
        {
            //Arrange
            string value = "1475594054551";

            //Act
            var result = value.ToDateTime();

            //Assert
            Assert.AreEqual(4, result.Value.Day);
            Assert.AreEqual(10, result.Value.Month);
            Assert.AreEqual(2016, result.Value.Year);
            Assert.AreEqual(15, result.Value.Hour);
            Assert.AreEqual(14, result.Value.Minute);
            Assert.AreEqual(0, result.Value.Second);
            Assert.AreEqual(0, result.Value.Millisecond);
        }

        [TestMethod]
        public void ConvertTimeStampStringToDateTimeShouldReturnNullIfInvalid()
        {
            //Arrange
            string value = "147559s054551";

            //Act
            var result = value.ToDateTime();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TruncateToMinutesSuccessfully()
        {
            //Arrange
            var date = new DateTime(2016, 12, 10, 11, 55, 23, 120);
            
            //Act
            var result = date.Truncate(TimeSpan.FromMinutes(1));

            //Assert
            Assert.AreEqual(2016, result.Year);
            Assert.AreEqual(12, result.Month);
            Assert.AreEqual(10, result.Day);
            Assert.AreEqual(11, result.Hour);
            Assert.AreEqual(55, result.Minute);
            Assert.AreEqual(0, result.Second);
            Assert.AreEqual(0, result.Millisecond);
        }
    }
}
