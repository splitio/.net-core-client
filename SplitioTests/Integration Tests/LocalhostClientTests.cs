using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;
using System;
using System.IO;
using System.Threading;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class LocalhostClientTests
    {
        [DeploymentItem(@"Resources\test.splits")]
        [TestMethod]
        public void GetTreatmentSuccessfully()
        {
            //Arrange
            var client = new LocalhostClient("test.splits");

            //Act
            var result1 = client.GetTreatment("", "double_writes_to_cassandra");
            var result2 = client.GetTreatment("id", "double_writes_to_cassandra");
            var result3 = client.GetTreatment("", "other_test_feature");
            var result4 = client.GetTreatment("id", "other_test_feature");

            //Asert
            Assert.IsTrue(result1 == "off"); //default treatment
            Assert.IsTrue(result2 == "off"); //default treatment
            Assert.IsTrue(result3 == "on"); //default treatment
            Assert.IsTrue(result4 == "on"); //default treatment
        }


        [DeploymentItem(@"Resources\test.splits")]
        [TestMethod]
        public void GetTreatmentSuccessfullyWhenUpdatingSplitsFile()
        {
            //Arrange
            var client = new LocalhostClient("test.splits");
            File.AppendAllText("test.splits", Environment.NewLine +"other_test_feature2     off" + Environment.NewLine);
            Thread.Sleep(10);

            //Act
            var result1 = client.GetTreatment("", "double_writes_to_cassandra");
            var result2 = client.GetTreatment("id", "double_writes_to_cassandra");
            var result3 = client.GetTreatment("", "other_test_feature");
            var result4 = client.GetTreatment("id", "other_test_feature");
            var result5 = client.GetTreatment("", "other_test_feature2");
            var result6 = client.GetTreatment("id", "other_test_feature2");

            //Assert
            Assert.IsTrue(result1 == "off"); //default treatment
            Assert.IsTrue(result2 == "off"); //default treatment
            Assert.IsTrue(result3 == "on"); //default treatment
            Assert.IsTrue(result4 == "on"); //default treatment
            Assert.IsTrue(result5 == "off"); //default treatment
            Assert.IsTrue(result6 == "off"); //default treatment
        }
    }
}
