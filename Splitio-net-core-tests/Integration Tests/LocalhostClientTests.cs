using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Client.Classes;
using System;
using System.IO;
using System.Threading;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class LocalhostClientTests
    {
        private Mock<ILog> _logMock = new Mock<ILog>();

        [DeploymentItem(@"Resources\test.splits")]
        [TestMethod]
        public void GetTreatmentSuccessfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\test.splits", _logMock.Object);

            //Act
            var result1 = client.GetTreatment("id", "double_writes_to_cassandra");
            var result2 = client.GetTreatment("id", "double_writes_to_cassandra");
            var result3 = client.GetTreatment("id", "other_test_feature");
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
            var client = new LocalhostClient(@"Resources\test.splits", _logMock.Object);
            File.AppendAllText(@"Resources\test.splits", Environment.NewLine +"other_test_feature2     off" + Environment.NewLine);
            Thread.Sleep(50);

            //Act
            var result1 = client.GetTreatment("id", "double_writes_to_cassandra");
            var result2 = client.GetTreatment("id", "double_writes_to_cassandra");
            var result3 = client.GetTreatment("id", "other_test_feature");
            var result4 = client.GetTreatment("id", "other_test_feature");
            var result5 = client.GetTreatment("id", "other_test_feature2");
            var result6 = client.GetTreatment("id", "other_test_feature2");

            //Assert
            Assert.IsTrue(result1 == "off"); //default treatment
            Assert.IsTrue(result2 == "off"); //default treatment
            Assert.IsTrue(result3 == "on"); //default treatment
            Assert.IsTrue(result4 == "on"); //default treatment
            Assert.IsTrue(result5 == "off"); //default treatment
            Assert.IsTrue(result6 == "off"); //default treatment
        }

        [DeploymentItem(@"Resources\test.splits")]
        [TestMethod]
        public void ClientDestroySuccessfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\test.splits", _logMock.Object);


            //Act
            var result1 = client.GetTreatment("id", "double_writes_to_cassandra");
            var result2 = client.GetTreatment("id", "double_writes_to_cassandra");


            client.Destroy();

            var resultDestroy1 = client.GetTreatment("id", "double_writes_to_cassandra");
            var manager = client.GetSplitManager();
            var resultDestroy2 = manager.Splits();
            var resultDestroy3 = manager.SplitNames();
            var resultDestroy4 = manager.Split("double_writes_to_cassandra");


            //Asert
            Assert.IsTrue(result1 == "off");
            Assert.IsTrue(result2 == "off");
            Assert.IsTrue(resultDestroy1 == "control");
            Assert.AreEqual(resultDestroy2.Count, 0);
            Assert.AreEqual(resultDestroy3.Count, 0);
            Assert.IsTrue(resultDestroy4 == null);

        }
    }
}
