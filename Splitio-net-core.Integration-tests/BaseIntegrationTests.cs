using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Impressions.Interfaces;
using Splitio_net_core.Integration_tests.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio_net_core.Integration_tests
{
    [DeploymentItem(@"Resources\split_changes.json")]
    [DeploymentItem(@"Resources\split_changes_1.json")]
    [DeploymentItem(@"Resources\split_segment1.json")]
    [DeploymentItem(@"Resources\split_segment2.json")]
    [DeploymentItem(@"Resources\split_segment3.json")]
    [TestClass]
    public abstract class BaseIntegrationTests
    {
        protected IImpressionListener _impressionListener;
                
        #region GetTreatment        
        [TestMethod]
        public void GetTreatment_WithtBUR_WithMultipleCalls_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var result1 = client.GetTreatment("nico_test", "FACUNDO_TEST");
            var result2 = client.GetTreatment("mauro_test", "FACUNDO_TEST");
            var result3 = client.GetTreatment("1", "Test_Save_1");
            var result4 = client.GetTreatment("24", "Test_Save_1");

            // Assert.
            Assert.AreEqual("on", result1);
            Assert.AreEqual("off", result2);
            Assert.AreEqual("on", result3);
            Assert.AreEqual("off", result4);

            // Validate impressions in listener.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(4, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("mauro_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("1"))
                .First();

            var impression4 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("24"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1506703262916, "FACUNDO_TEST", "mauro_test", "in segment all", "off");
            AssertImpression(impression3, 1503956389520, "Test_Save_1", "1", "whitelisted", "on");
            AssertImpression(impression4, 1503956389520, "Test_Save_1", "24", "in segment all", "off");

            //Validate impressions sent to the be.
            AssertSentImpressions(4, httpClientMock, impression1, impression2, impression3, impression4);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatment_WithtInputValidation_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var result1 = client.GetTreatment("nico_test", "FACUNDO_TEST");
            var result2 = client.GetTreatment(string.Empty, "FACUNDO_TEST");
            var result3 = client.GetTreatment("1", string.Empty);
            var result4 = client.GetTreatment("24", "Test_Save_1");

            // Assert.
            Assert.AreEqual("on", result1);
            Assert.AreEqual("control", result2);
            Assert.AreEqual("control", result3);
            Assert.AreEqual("off", result4);

            // Validate impressions in listener.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(2, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("24"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1503956389520, "Test_Save_1", "24", "in segment all", "off");

            //Validate impressions sent to the be.
            AssertSentImpressions(2, httpClientMock, impression1, impression2);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatment_WithtBUR_WhenTreatmentDoesntExist_ReturnsControl()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var result = client.GetTreatment("nico_test", "Random_Treatment");

            // Assert.
            Assert.AreEqual("control", result);

            // Validate impressions in listener.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(0, keyImpressions.Count);

            //Validate impressions sent to the be.
            AssertSentImpressions(0, httpClientMock);

            ShutdownServer(httpClientMock);
        }
        #endregion

        #region GetTreatmentWithConfig        
        [TestMethod]
        public void GetTreatmentWithConfig_WithtBUR_WithMultipleCalls_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch
            {
                client.BlockUntilReady(100000);
            }

            // Act.
            var result1 = client.GetTreatmentWithConfig("nico_test", "FACUNDO_TEST");
            var result2 = client.GetTreatmentWithConfig("mauro_test", "FACUNDO_TEST");
            var result3 = client.GetTreatmentWithConfig("mauro", "MAURO_TEST");
            var result4 = client.GetTreatmentWithConfig("test", "MAURO_TEST");

            // Assert.
            Assert.AreEqual("on", result1.Treatment);
            Assert.AreEqual("off", result2.Treatment);
            Assert.AreEqual("on", result3.Treatment);
            Assert.AreEqual("off", result4.Treatment);

            Assert.AreEqual("{\"color\":\"green\"}", result1.Config);
            Assert.IsNull(result2.Config);
            Assert.AreEqual("{\"version\":\"v2\"}", result3.Config);
            Assert.AreEqual("{\"version\":\"v1\"}", result4.Config);

            // Validate impressions.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(4, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("mauro_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("mauro"))
                .First();

            var impression4 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("test"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1506703262916, "FACUNDO_TEST", "mauro_test", "in segment all", "off");
            AssertImpression(impression3, 1506703262966, "MAURO_TEST", "mauro", "whitelisted", "on");
            AssertImpression(impression4, 1506703262966, "MAURO_TEST", "test", "not in split", "off");

            //Validate impressions sent to the be.
            AssertSentImpressions(4, httpClientMock, impression1, impression2, impression3, impression4);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WithtInputValidation_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch
            {
                client.BlockUntilReady(100000);
            }

            // Act.
            var result1 = client.GetTreatmentWithConfig("nico_test", "FACUNDO_TEST");
            var result2 = client.GetTreatmentWithConfig(string.Empty, "FACUNDO_TEST");
            var result3 = client.GetTreatmentWithConfig("test", string.Empty);
            var result4 = client.GetTreatmentWithConfig("mauro", "MAURO_TEST");

            // Assert.
            Assert.AreEqual("on", result1.Treatment);
            Assert.AreEqual("control", result2.Treatment);
            Assert.AreEqual("control", result3.Treatment);
            Assert.AreEqual("on", result4.Treatment);

            Assert.AreEqual("{\"color\":\"green\"}", result1.Config);
            Assert.IsNull(result2.Config);
            Assert.IsNull(result3.Config);
            Assert.AreEqual("{\"version\":\"v2\"}", result4.Config);

            // Validate impressions.
            Thread.Sleep(3000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(2, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("mauro"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1506703262966, "MAURO_TEST", "mauro", "whitelisted", "on");

            //Validate impressions sent to the be.
            AssertSentImpressions(2, httpClientMock, impression1, impression2);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WithtBUR_WhenTreatmentDoesntExist_ReturnsControl()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var result = client.GetTreatment("nico_test", "Random_Treatment");

            // Assert.
            Assert.AreEqual("control", result);

            // Validate impressions in listener.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(0, keyImpressions.Count);

            //Validate impressions sent to the be.
            AssertSentImpressions(0, httpClientMock);

            ShutdownServer(httpClientMock);
        }
        #endregion

        #region GetTreatments
        [TestMethod]
        public void GetTreatments_WithtBUR_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch
            {
                client.BlockUntilReady(100000);
            }

            // Act.
            var result = client.GetTreatments("nico_test", new List<string> { "FACUNDO_TEST", "MAURO_TEST", "Test_Save_1" });

            // Assert.
            Assert.AreEqual("on", result["FACUNDO_TEST"]);
            Assert.AreEqual("off", result["MAURO_TEST"]);
            Assert.AreEqual("off", result["Test_Save_1"]);

            // Validate impressions.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(3, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1506703262966, "MAURO_TEST", "nico_test", "not in split", "off");
            AssertImpression(impression3, 1503956389520, "Test_Save_1", "nico_test", "in segment all", "off");

            //Validate impressions sent to the be.            
            AssertSentImpressions(3, httpClientMock, impression1, impression2, impression3);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatments_WithtInputValidation_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch
            {
                client.BlockUntilReady(100000);
            }

            // Act.
            var result1 = client.GetTreatments("nico_test", new List<string> { "FACUNDO_TEST", string.Empty, "Test_Save_1" });
            var result2 = client.GetTreatments("mauro", new List<string> { string.Empty, "MAURO_TEST", "Test_Save_1" });
            var result3 = client.GetTreatments(string.Empty, new List<string> { "FACUNDO_TEST", "MAURO_TEST", "Test_Save_1" });

            // Assert.
            Assert.AreEqual("on", result1["FACUNDO_TEST"]);
            Assert.AreEqual("off", result1["Test_Save_1"]);
            Assert.AreEqual("on", result2["MAURO_TEST"]);
            Assert.AreEqual("off", result2["Test_Save_1"]);
            Assert.AreEqual("control", result3["FACUNDO_TEST"]);
            Assert.AreEqual("control", result3["MAURO_TEST"]);
            Assert.AreEqual("control", result3["Test_Save_1"]);

            // Validate impressions.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(4, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("mauro"))
                .First();

            var impression4 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("mauro"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1503956389520, "Test_Save_1", "nico_test", "in segment all", "off");
            AssertImpression(impression3, 1506703262966, "MAURO_TEST", "mauro", "whitelisted", "on");
            AssertImpression(impression4, 1503956389520, "Test_Save_1", "mauro", "in segment all", "off");

            //Validate impressions sent to the be.
            AssertSentImpressions(4, httpClientMock, impression1, impression2, impression3, impression4);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatments_WithtBUR_WhenTreatmentsDoesntExist_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch
            {
                client.BlockUntilReady(100000);
            }

            // Act.
            var result = client.GetTreatments("nico_test", new List<string> { "FACUNDO_TEST", "Random_Treatment", "MAURO_TEST", "Test_Save_1", "Random_Treatment_2", });

            // Assert.
            Assert.AreEqual("on", result["FACUNDO_TEST"]);
            Assert.AreEqual("control", result["Random_Treatment"]);
            Assert.AreEqual("off", result["MAURO_TEST"]);
            Assert.AreEqual("off", result["Test_Save_1"]);
            Assert.AreEqual("control", result["Random_Treatment_2"]);

            // Validate impressions.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(3, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1506703262966, "MAURO_TEST", "nico_test", "not in split", "off");
            AssertImpression(impression3, 1503956389520, "Test_Save_1", "nico_test", "in segment all", "off");

            //Validate impressions sent to the be.            
            AssertSentImpressions(3, httpClientMock, impression1, impression2, impression3);

            ShutdownServer(httpClientMock);
        }
        #endregion

        #region GetTreatmentsWithConfig
        [TestMethod]
        public void GetTreatmentsWithConfig_WithtBUR_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var result = client.GetTreatmentsWithConfig("nico_test", new List<string> { "FACUNDO_TEST", "MAURO_TEST", "Test_Save_1" });

            // Assert.
            Assert.AreEqual("on", result["FACUNDO_TEST"].Treatment);
            Assert.AreEqual("off", result["MAURO_TEST"].Treatment);
            Assert.AreEqual("off", result["Test_Save_1"].Treatment);

            Assert.AreEqual("{\"color\":\"green\"}", result["FACUNDO_TEST"].Config);
            Assert.AreEqual("{\"version\":\"v1\"}", result["MAURO_TEST"].Config);
            Assert.IsNull(result["Test_Save_1"].Config);

            // Validate impressions.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(3, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1506703262966, "MAURO_TEST", "nico_test", "not in split", "off");
            AssertImpression(impression3, 1503956389520, "Test_Save_1", "nico_test", "in segment all", "off");

            //Validate impressions sent to the be.
            AssertSentImpressions(3, httpClientMock, impression1, impression2, impression3);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_WithtInputValidation_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey2";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch
            {
                client.BlockUntilReady(100000);
            }

            // Act.
            var result1 = client.GetTreatmentsWithConfig("nico_test", new List<string> { "FACUNDO_TEST", string.Empty, "Test_Save_1" });
            var result2 = client.GetTreatmentsWithConfig("mauro", new List<string> { string.Empty, "MAURO_TEST", "Test_Save_1" });
            var result3 = client.GetTreatmentsWithConfig(string.Empty, new List<string> { "FACUNDO_TEST", "MAURO_TEST", "Test_Save_1" });

            // Assert.
            Assert.AreEqual("on", result1["FACUNDO_TEST"].Treatment);
            Assert.AreEqual("off", result1["Test_Save_1"].Treatment);
            Assert.AreEqual("on", result2["MAURO_TEST"].Treatment);
            Assert.AreEqual("off", result2["Test_Save_1"].Treatment);
            Assert.AreEqual("control", result3["FACUNDO_TEST"].Treatment);
            Assert.AreEqual("control", result3["MAURO_TEST"].Treatment);
            Assert.AreEqual("control", result3["Test_Save_1"].Treatment);

            Assert.AreEqual("{\"color\":\"green\"}", result1["FACUNDO_TEST"].Config);
            Assert.IsNull(result1["Test_Save_1"].Config);
            Assert.AreEqual("{\"version\":\"v2\"}", result2["MAURO_TEST"].Config);
            Assert.IsNull(result2["Test_Save_1"].Config);
            Assert.IsNull(result3["FACUNDO_TEST"].Config);
            Assert.IsNull(result3["MAURO_TEST"].Config);
            Assert.IsNull(result3["Test_Save_1"].Config);

            // Validate impressions.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(4, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("mauro"))
                .First();

            var impression4 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("mauro"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1503956389520, "Test_Save_1", "nico_test", "in segment all", "off");
            AssertImpression(impression3, 1506703262966, "MAURO_TEST", "mauro", "whitelisted", "on");
            AssertImpression(impression4, 1503956389520, "Test_Save_1", "mauro", "in segment all", "off");

            //Validate impressions sent to the be.
            AssertSentImpressions(4, httpClientMock, impression1, impression2, impression3, impression4);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_WithtBUR_WhenTreatmentsDoesntExist_ReturnsTreatments()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch
            {
                client.BlockUntilReady(100000);
            }

            // Act.
            var result = client.GetTreatmentsWithConfig("nico_test", new List<string> { "FACUNDO_TEST", "Random_Treatment", "MAURO_TEST", "Test_Save_1", "Random_Treatment_1" });

            // Assert.
            Assert.AreEqual("on", result["FACUNDO_TEST"].Treatment);
            Assert.AreEqual("control", result["Random_Treatment"].Treatment);
            Assert.AreEqual("off", result["MAURO_TEST"].Treatment);
            Assert.AreEqual("off", result["Test_Save_1"].Treatment);
            Assert.AreEqual("control", result["Random_Treatment_1"].Treatment);

            Assert.AreEqual("{\"color\":\"green\"}", result["FACUNDO_TEST"].Config);
            Assert.AreEqual("{\"version\":\"v1\"}", result["MAURO_TEST"].Config);
            Assert.IsNull(result["Test_Save_1"].Config);

            // Validate impressions.
            Thread.Sleep(2000);
            var impressionQueue = ((IntegrationTestsImpressionListener)_impressionListener).GetQueue();
            var keyImpressions = impressionQueue.FetchAll();

            Assert.AreEqual(3, keyImpressions.Count);

            var impression1 = keyImpressions
                .Where(ki => ki.feature.Equals("FACUNDO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression2 = keyImpressions
                .Where(ki => ki.feature.Equals("MAURO_TEST"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            var impression3 = keyImpressions
                .Where(ki => ki.feature.Equals("Test_Save_1"))
                .Where(ki => ki.keyName.Equals("nico_test"))
                .First();

            AssertImpression(impression1, 1506703262916, "FACUNDO_TEST", "nico_test", "whitelisted", "on");
            AssertImpression(impression2, 1506703262966, "MAURO_TEST", "nico_test", "not in split", "off");
            AssertImpression(impression3, 1503956389520, "Test_Save_1", "nico_test", "in segment all", "off");

            //Validate impressions sent to the be.
            AssertSentImpressions(3, httpClientMock, impression1, impression2, impression3);

            ShutdownServer(httpClientMock);
        }
        #endregion

        #region Manager
        [TestMethod]
        public void Manager_SplitNames_ReturnsSplitNames()
        {
            // Arrange.
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey22";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            var manager = client.GetSplitManager();

            // Act.
            var result = manager.SplitNames();

            // Assert.
            Assert.AreEqual(30, result.Count);
            Assert.IsInstanceOfType(result, typeof(List<string>));

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void Manager_Splits_ReturnsSplitList()
        {
            // Arrange.
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey23";

            var splitFactory = new SplitFactory(apikey, configurations);
            var manager = splitFactory.Manager();

            manager.BlockUntilReady(10000);

            // Act.
            var result = manager.Splits();

            // Assert.
            Assert.AreEqual(30, result.Count);
            Assert.IsInstanceOfType(result, typeof(List<SplitView>));

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void Manager_Split_ReturnsSplit()
        {
            // Arrange.
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var splitName = "MAURO_TEST";
            var apikey = "apikey24";

            var splitFactory = new SplitFactory(apikey, configurations);
            var manager = splitFactory.Manager();

            manager.BlockUntilReady(10000);

            // Act.
            var result = manager.Split(splitName);

            // Assert.
            Assert.IsNotNull(result);
            Assert.AreEqual(splitName, result.name);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void Manager_Split_WhenNameDoesntExist_ReturnsSplit()
        {
            // Arrange.
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var splitName = "Split_Name";
            var apikey = "apikey25";

            var splitFactory = new SplitFactory(apikey, configurations);
            var manager = splitFactory.Manager();

            manager.BlockUntilReady(10000);

            // Act.
            var result = manager.Split(splitName);

            // Assert.
            Assert.IsNull(result);

            ShutdownServer(httpClientMock);
        }
        #endregion

        #region Track
        [TestMethod]
        public void Track_WithValidData_ReturnsTrue()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var properties = new Dictionary<string, object>
            {
                { "property_1",  1 },
                { "property_2",  2 }
            };

            var events = new List<EventBackend>
            {
                new EventBackend { Key = "key_1", TrafficTypeName = "traffic_type_1", EventTypeId = "event_type_1", Value = 123, Properties = properties },
                new EventBackend { Key = "key_2", TrafficTypeName = "traffic_type_2", EventTypeId = "event_type_2", Value = 222 },
                new EventBackend { Key = "key_3", TrafficTypeName = "traffic_type_3", EventTypeId = "event_type_3", Value = 333 },
                new EventBackend { Key = "key_4", TrafficTypeName = "traffic_type_4", EventTypeId = "event_type_4", Value = 444, Properties = properties }
            };

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            foreach (var _event in events)
            {
                // Act.
                var result = client.Track(_event.Key, _event.TrafficTypeName, _event.EventTypeId, _event.Value, _event.Properties);

                // Assert. 
                Assert.IsTrue(result);
            }

            //Validate Events sent to the be.
            AssertSentEvents(events, httpClientMock);
        }

        [TestMethod]
        public void Track_WithBUR_ReturnsTrue()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var properties = new Dictionary<string, object>
            {
                { "property_1",  1 },
                { "property_2",  2 }
            };

            var events = new List<EventBackend>
            {
                new EventBackend { Key = "key_1", TrafficTypeName = "traffic_type_1", EventTypeId = "event_type_1", Value = 123, Properties = properties },
                new EventBackend { Key = "key_2", TrafficTypeName = "traffic_type_2", EventTypeId = "event_type_2", Value = 222 },
                new EventBackend { Key = "key_3", TrafficTypeName = "traffic_type_3", EventTypeId = "event_type_3", Value = 333 },
                new EventBackend { Key = "key_4", TrafficTypeName = "traffic_type_4", EventTypeId = "event_type_4", Value = 444, Properties = properties }
            };

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            foreach (var _event in events)
            {
                // Act.
                var result = client.Track(_event.Key, _event.TrafficTypeName, _event.EventTypeId, _event.Value, _event.Properties);

                // Assert. 
                Assert.IsTrue(result);
            }

            //Validate Events sent to the be.
            AssertSentEvents(events, httpClientMock);
        }

        [TestMethod]
        public void Track_WithInvalidData_ReturnsFalse()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var properties = new Dictionary<string, object>
            {
                { "property_1",  1 },
                { "property_2",  2 }
            };

            var events = new List<EventBackend>
            {
                new EventBackend { Key = string.Empty, TrafficTypeName = "traffic_type_1", EventTypeId = "event_type_1", Value = 123, Properties = properties },
                new EventBackend { Key = "key_2", TrafficTypeName = string.Empty, EventTypeId = "event_type_2", Value = 222 },
                new EventBackend { Key = "key_3", TrafficTypeName = "traffic_type_3", EventTypeId = string.Empty, Value = 333 },
                new EventBackend { Key = "key_4", TrafficTypeName = "traffic_type_4", EventTypeId = "event_type_4", Value = 444, Properties = properties },
                new EventBackend { Key = "key_5", TrafficTypeName = "traffic_type_5", EventTypeId = "event_type_5"}
            };

            var apikey = "apikey33";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            foreach (var _event in events)
            {
                // Act.
                var result = client.Track(_event.Key, _event.TrafficTypeName, _event.EventTypeId, _event.Value, _event.Properties);

                // Assert. 
                if (string.IsNullOrEmpty(_event.Key) || _event.Key.Equals("key_2") || _event.Key.Equals("key_3"))
                    Assert.IsFalse(result);
                else
                    Assert.IsTrue(result);
            }
                        
            events = events
                .Where(e => e.Key.Equals("key_4") || e.Key.Equals("key_5"))
                .ToList();
            
            //Validate Events sent to the be.
            AssertSentEvents(events, httpClientMock);
        }

        [TestMethod]
        public void Track_WithLowQueue_ReturnsTrue()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock, eventsPushRate: 60, eventsQueueSize: 3);

            var properties = new Dictionary<string, object>
            {
                { "property_1",  1 },
                { "property_2",  2 }
            };

            var events = new List<EventBackend>
            {
                new EventBackend { Key = "key_1", TrafficTypeName = "traffic_type_1", EventTypeId = "event_type_1", Value = 123, Properties = properties },
                new EventBackend { Key = "key_2", TrafficTypeName = "traffic_type_2", EventTypeId = "event_type_2", Value = 222 },
                new EventBackend { Key = "key_3", TrafficTypeName = "traffic_type_3", EventTypeId = "event_type_3", Value = 333 },
                new EventBackend { Key = "key_4", TrafficTypeName = "traffic_type_4", EventTypeId = "event_type_4", Value = 444, Properties = properties },
                new EventBackend { Key = "key_5", TrafficTypeName = "traffic_type_5", EventTypeId = "event_type_5"}
            };

            var apikey = "apikey53";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            foreach (var _event in events)
            {
                // Act.
                var result = client.Track(_event.Key, _event.TrafficTypeName, _event.EventTypeId, _event.Value, _event.Properties);

                // Assert. 
                Assert.IsTrue(result);
            }

            //Validate Events sent to the be.
            AssertSentEvents(events, httpClientMock, sleepTime: 3000, eventsCount: 3, validateEvents: false);
        }
        #endregion

        #region Destroy
        [TestMethod]
        public void Destroy()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            var manager = client.GetSplitManager();

            // Act.
            var treatmentResult = client.GetTreatment("nico_test", "FACUNDO_TEST");
            var managerResult = manager.Split("MAURO_TEST");

            client.Destroy();

            var destroyResult = client.GetTreatment("nico_test", "FACUNDO_TEST");
            var managerDestroyResult = manager.Split("MAURO_TEST");

            // Assert.
            Assert.AreEqual("on", treatmentResult);
            Assert.AreEqual("control", destroyResult);
            Assert.IsTrue(client.IsDestroyed());

            Assert.IsNotNull(managerResult);
            Assert.AreEqual("MAURO_TEST", managerResult.name);
            // TODO : Redis destroy doesn't work. Refactor this and uncomment assert
            //Assert.IsNull(managerDestroyResult);

            ShutdownServer(httpClientMock);
        }
        #endregion

        #region Protected Methods
        protected void AssertImpression(KeyImpression impression, long changeNumber, string feature, string keyName, string label, string treatment)
        {
            Assert.AreEqual(changeNumber, impression.changeNumber);
            Assert.AreEqual(feature, impression.feature);
            Assert.AreEqual(keyName, impression.keyName);
            Assert.AreEqual(label, impression.label);
            Assert.AreEqual(treatment, impression.treatment);
        }
        
        protected abstract ConfigurationOptions GetConfigurationOptions(HttpClientMock httpClientMock = null, int? eventsPushRate = null, int? eventsQueueSize = null, int? featuresRefreshRate = null, bool? ipAddressesEnabled = null);

        protected abstract void ShutdownServer(HttpClientMock httpClientMock = null);

        protected abstract void AssertSentImpressions(int sentImpressionsCount, HttpClientMock httpClientMock = null, params KeyImpression[] expectedImpressions);

        protected abstract void AssertSentEvents(List<EventBackend> eventsExcpected, HttpClientMock httpClientMock = null, int sleepTime = 15000, int? eventsCount = null, bool validateEvents = true);

        protected abstract HttpClientMock GetHttpClientMock();
        #endregion
    }
}