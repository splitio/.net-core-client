using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Client.Classes;
using System;
using System.Collections.Generic;
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

            client.BlockUntilReady(1000);

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

            client.BlockUntilReady(1000);

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

            client.BlockUntilReady(1000);

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

        [DeploymentItem(@"Resources\split.yaml")]
        [TestMethod]
        public void GetTreatment_WhenIsYamlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yaml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var result = client.GetTreatment("id", "testing_split_on");
            Assert.AreEqual("on", result);

            result = client.GetTreatment("key_for_wl", "testing_split_only_wl");
            Assert.AreEqual("whitelisted", result);

            result = client.GetTreatment("id", "testing_split_with_wl");
            Assert.AreEqual("not_in_whitelist", result);

            result = client.GetTreatment("key_for_wl", "testing_split_with_wl");
            Assert.AreEqual("one_key_wl", result);

            result = client.GetTreatment("key_for_wl_1", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result);

            result = client.GetTreatment("key_for_wl_2", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result);

            result = client.GetTreatment("key_for_wl_2", "testing_split_off_with_config");
            Assert.AreEqual("off", result);
        }

        [DeploymentItem(@"Resources\split.yaml")]
        [TestMethod]
        public void GetTreatmentWithConfig_WhenIsYamlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yaml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var result = client.GetTreatmentWithConfig("id", "testing_split_on");
            Assert.AreEqual("on", result.Treatment);
            Assert.IsNull(result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl", "testing_split_only_wl");
            Assert.AreEqual("whitelisted", result.Treatment);
            Assert.IsNull(result.Config);

            result = client.GetTreatmentWithConfig("id", "testing_split_with_wl");
            Assert.AreEqual("not_in_whitelist", result.Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl", "testing_split_with_wl");
            Assert.AreEqual("one_key_wl", result.Treatment);
            Assert.IsNull(result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl_1", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result.Treatment);
            Assert.AreEqual("{\"color\": \"brown\"}", result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl_2", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result.Treatment);
            Assert.AreEqual("{\"color\": \"brown\"}", result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl_2", "testing_split_off_with_config");
            Assert.AreEqual("off", result.Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", result.Config);
        }

        [DeploymentItem(@"Resources\split.yml")]
        [TestMethod]
        public void GetTreatment_WhenIsYmlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var result = client.GetTreatment("id", "testing_split_on");
            Assert.AreEqual("on", result);

            result = client.GetTreatment("key_for_wl", "testing_split_only_wl");
            Assert.AreEqual("whitelisted", result);

            result = client.GetTreatment("id", "testing_split_with_wl");
            Assert.AreEqual("not_in_whitelist", result);

            result = client.GetTreatment("key_for_wl", "testing_split_with_wl");
            Assert.AreEqual("one_key_wl", result);

            result = client.GetTreatment("key_for_wl_1", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result);

            result = client.GetTreatment("key_for_wl_2", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result);

            result = client.GetTreatment("key_for_wl_2", "testing_split_off_with_config");
            Assert.AreEqual("off", result);
        }

        [DeploymentItem(@"Resources\split.yml")]
        [TestMethod]
        public void GetTreatmentWithConfig_WhenIsYmlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var result = client.GetTreatmentWithConfig("id", "testing_split_on");
            Assert.AreEqual("on", result.Treatment);
            Assert.IsNull(result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl", "testing_split_only_wl");
            Assert.AreEqual("whitelisted", result.Treatment);
            Assert.IsNull(result.Config);

            result = client.GetTreatmentWithConfig("id", "testing_split_with_wl");
            Assert.AreEqual("not_in_whitelist", result.Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl", "testing_split_with_wl");
            Assert.AreEqual("one_key_wl", result.Treatment);
            Assert.IsNull(result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl_1", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result.Treatment);
            Assert.AreEqual("{\"color\": \"brown\"}", result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl_2", "testing_split_with_wl");
            Assert.AreEqual("multi_key_wl", result.Treatment);
            Assert.AreEqual("{\"color\": \"brown\"}", result.Config);

            result = client.GetTreatmentWithConfig("key_for_wl_2", "testing_split_off_with_config");
            Assert.AreEqual("off", result.Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", result.Config);
        }

        [DeploymentItem(@"Resources\split.yaml")]
        [TestMethod]
        public void GetTreatments_WhenIsYamlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yaml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var results = client.GetTreatments("id", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"]);
            Assert.AreEqual("control", results["testing_split_only_wl"]);
            Assert.AreEqual("not_in_whitelist", results["testing_split_with_wl"]);
            Assert.AreEqual("off", results["testing_split_off_with_config"]);

            results = client.GetTreatments("key_for_wl", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"]);
            Assert.AreEqual("whitelisted", results["testing_split_only_wl"]);
            Assert.AreEqual("one_key_wl", results["testing_split_with_wl"]);
            Assert.AreEqual("off", results["testing_split_off_with_config"]);
        }

        [DeploymentItem(@"Resources\split.yaml")]
        [TestMethod]
        public void GetTreatmentsWithConfig_WhenIsYamlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yaml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var results = client.GetTreatmentsWithConfig("id", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"].Treatment);
            Assert.IsNull(results["testing_split_on"].Config);

            Assert.AreEqual("control", results["testing_split_only_wl"].Treatment);
            Assert.IsNull(results["testing_split_on"].Config);

            Assert.AreEqual("not_in_whitelist", results["testing_split_with_wl"].Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", results["testing_split_with_wl"].Config);

            Assert.AreEqual("off", results["testing_split_off_with_config"].Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", results["testing_split_off_with_config"].Config);

            results = client.GetTreatmentsWithConfig("key_for_wl", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"].Treatment);
            Assert.IsNull(results["testing_split_on"].Config);            

            Assert.AreEqual("whitelisted", results["testing_split_only_wl"].Treatment);
            Assert.IsNull(results["testing_split_only_wl"].Config);

            Assert.AreEqual("one_key_wl", results["testing_split_with_wl"].Treatment);
            Assert.IsNull(results["testing_split_with_wl"].Config);

            Assert.AreEqual("off", results["testing_split_off_with_config"].Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", results["testing_split_off_with_config"].Config);
        }

        [DeploymentItem(@"Resources\split.yml")]
        [TestMethod]
        public void GetTreatments_WhenIsYmlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var results = client.GetTreatments("id", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"]);
            Assert.AreEqual("control", results["testing_split_only_wl"]);
            Assert.AreEqual("not_in_whitelist", results["testing_split_with_wl"]);
            Assert.AreEqual("off", results["testing_split_off_with_config"]);

            results = client.GetTreatments("key_for_wl", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"]);
            Assert.AreEqual("whitelisted", results["testing_split_only_wl"]);
            Assert.AreEqual("one_key_wl", results["testing_split_with_wl"]);
            Assert.AreEqual("off", results["testing_split_off_with_config"]);
        }

        [DeploymentItem(@"Resources\split.yml")]
        [TestMethod]
        public void GetTreatmentsWithConfig_WhenIsYmlFile_Successfully()
        {
            //Arrange
            var client = new LocalhostClient(@"Resources\split.yml", _logMock.Object);

            client.BlockUntilReady(1000);

            //Act
            var results = client.GetTreatmentsWithConfig("id", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"].Treatment);
            Assert.IsNull(results["testing_split_on"].Config);

            Assert.AreEqual("control", results["testing_split_only_wl"].Treatment);
            Assert.IsNull(results["testing_split_on"].Config);

            Assert.AreEqual("not_in_whitelist", results["testing_split_with_wl"].Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", results["testing_split_with_wl"].Config);

            Assert.AreEqual("off", results["testing_split_off_with_config"].Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", results["testing_split_off_with_config"].Config);

            results = client.GetTreatmentsWithConfig("key_for_wl", new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config"
            });

            Assert.AreEqual("on", results["testing_split_on"].Treatment);
            Assert.IsNull(results["testing_split_on"].Config);

            Assert.AreEqual("whitelisted", results["testing_split_only_wl"].Treatment);
            Assert.IsNull(results["testing_split_only_wl"].Config);

            Assert.AreEqual("one_key_wl", results["testing_split_with_wl"].Treatment);
            Assert.IsNull(results["testing_split_with_wl"].Config);

            Assert.AreEqual("off", results["testing_split_off_with_config"].Treatment);
            Assert.AreEqual("{\"color\": \"green\"}", results["testing_split_off_with_config"].Config);
        }
    }
}
