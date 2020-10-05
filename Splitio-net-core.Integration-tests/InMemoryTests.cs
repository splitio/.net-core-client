using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio_net_core.Integration_tests.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio_net_core.Integration_tests
{
    [TestClass]
    public class InMemoryTests : BaseIntegrationTests
    {
        [TestMethod]
        public void GetTreatment_WithoutBUR_ReturnsControl()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            // Act.
            var treatmentResult = client.GetTreatment("nico_test", "FACUNDO_TEST");

            // Assert.
            Assert.AreEqual("control", treatmentResult);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WithoutBUR_ReturnsControl()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            // Act.
            var treatmentResult = client.GetTreatmentWithConfig("nico_test", "FACUNDO_TEST");

            // Assert.
            Assert.AreEqual("control", treatmentResult.Treatment);
            Assert.IsNull(treatmentResult.Config);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatments_WithoutBUR_ReturnsControl()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            // Act.
            var treatmentResults = client.GetTreatments("nico_test", new List<string> { "FACUNDO_TEST", "MAURO_TEST" });

            // Assert.            
            Assert.AreEqual("control", treatmentResults["FACUNDO_TEST"]);
            Assert.AreEqual("control", treatmentResults["MAURO_TEST"]);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_WithoutBUR_ReturnsControl()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            // Act.
            var treatmentResults = client.GetTreatmentsWithConfig("nico_test", new List<string> { "FACUNDO_TEST", "MAURO_TEST" });

            // Assert.            
            Assert.AreEqual("control", treatmentResults["FACUNDO_TEST"].Treatment);
            Assert.AreEqual("control", treatmentResults["MAURO_TEST"].Treatment);
            Assert.IsNull(treatmentResults["FACUNDO_TEST"].Config);
            Assert.IsNull(treatmentResults["MAURO_TEST"].Config);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatment_WithtBUR_ReturnsTimeOutException()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey2";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            // Act.
            var exceptionMessage = "";
            var isSdkReady = false;

            try
            {
                client.BlockUntilReady(1);
                isSdkReady = true;
            }
            catch (Exception ex)
            {
                isSdkReady = false;
                exceptionMessage = ex.Message;
            }

            // Assert.
            Assert.IsFalse(isSdkReady);
            Assert.AreEqual("SDK was not ready in 1 miliseconds", exceptionMessage);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void Manager_SplitNames_WithoutBUR_ReturnsNull()
        {
            // Arrange.
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey2";

            var splitFactory = new SplitFactory(apikey, configurations);
            var manager = splitFactory.Manager();
            
            // Act.
            var result = manager.SplitNames();

            // Assert.
            Assert.IsNull(result);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void CheckingHeaders_WithIPAddressesEnabled_ReturnsWithIpAndName()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var treatmentResult = client.GetTreatment("nico_test", "FACUNDO_TEST");

            // Assert.
            Assert.AreEqual("on", treatmentResult);

            Thread.Sleep(5000);

            var requests = httpClientMock.GetLogs();
            
            foreach (var req in requests)
            {
                Assert.IsTrue(req
                    .RequestMessage
                    .Headers
                    .Any(h => h.Key.Equals("SplitSDKMachineIP") || h.Key.Equals("SplitSDKMachineName")));
            }
            
            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void CheckingHeaders_WithIPAddressesDisabled_ReturnsWithoutIpAndName()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock, ipAddressesEnabled: false);

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var treatmentResult = client.GetTreatment("nico_test", "FACUNDO_TEST");

            // Assert.
            Assert.AreEqual("on", treatmentResult);

            Thread.Sleep(5000);

            var requests = httpClientMock.GetLogs();

            foreach (var req in requests)
            {
                Assert.IsFalse(req
                    .RequestMessage
                    .Headers
                    .Any(h => h.Key.Equals("SplitSDKMachineIP") || h.Key.Equals("SplitSDKMachineName")));
            }

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatments_ValidateDedupeImpressions_Optimized()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(20000);

            // Act.
            client.GetTreatmentWithConfig("nico_test", "FACUNDO_TEST");
            client.GetTreatmentWithConfig("nico_test", "FACUNDO_TEST");
            client.GetTreatmentWithConfig("test", "MAURO_TEST");
            client.GetTreatmentWithConfig("mauro", "MAURO_TEST");
            client.GetTreatmentWithConfig("mauro", "MAURO_TEST");
            client.GetTreatments("admin", new List<string> { "FACUNDO_TEST", "Test_Save_1" });
            client.GetTreatment("admin", "FACUNDO_TEST");
            client.GetTreatment("admin", "Test_Save_1");
            client.GetTreatmentsWithConfig("admin", new List<string> { "FACUNDO_TEST", "MAURO_TEST" });

            client.Destroy();
            Thread.Sleep(3000);

            // Assert.
            var sentImpressions = GetImpressionsSentBackend(httpClientMock);
            Assert.AreEqual(3, sentImpressions.Count);
            Assert.AreEqual(2, sentImpressions.FirstOrDefault(i => i.F.Equals("FACUNDO_TEST")).I.Count);
            Assert.AreEqual(3, sentImpressions.FirstOrDefault(i => i.F.Equals("MAURO_TEST")).I.Count);
            Assert.AreEqual(1, sentImpressions.FirstOrDefault(i => i.F.Equals("Test_Save_1")).I.Count);

            var impressionCounts = GetImpressionsCountsSentBackend(httpClientMock);
            Assert.AreEqual(3, impressionCounts.FirstOrDefault().Pf.Count);
            Assert.AreEqual(5, impressionCounts.FirstOrDefault().Pf.FirstOrDefault(i => i.F.Equals("FACUNDO_TEST")).Rc);
            Assert.AreEqual(4, impressionCounts.FirstOrDefault().Pf.FirstOrDefault(i => i.F.Equals("MAURO_TEST")).Rc);
            Assert.AreEqual(2, impressionCounts.FirstOrDefault().Pf.FirstOrDefault(i => i.F.Equals("Test_Save_1")).Rc);

            ShutdownServer(httpClientMock);
        }

        [TestMethod]
        public void GetTreatments_ValidateDedupeImpressions_Debug()
        {
            // Arrange.           
            var httpClientMock = GetHttpClientMock();
            var configurations = GetConfigurationOptions(httpClientMock);
            configurations.ImpressionMode = ImpressionModes.Debug;

            var apikey = "apikey3";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(20000);

            // Act.
            client.GetTreatmentWithConfig("nico_test", "FACUNDO_TEST");
            client.GetTreatmentWithConfig("nico_test", "FACUNDO_TEST");
            client.GetTreatmentWithConfig("test", "MAURO_TEST");
            client.GetTreatmentWithConfig("mauro", "MAURO_TEST");
            client.GetTreatments("admin", new List<string> { "FACUNDO_TEST", "Test_Save_1" });
            client.GetTreatment("admin", "FACUNDO_TEST");
            client.GetTreatmentsWithConfig("admin", new List<string> { "FACUNDO_TEST", "MAURO_TEST" });

            client.Destroy();
            Thread.Sleep(3000);

            // Assert.
            var sentImpressions = GetImpressionsSentBackend(httpClientMock);
            Assert.AreEqual(3, sentImpressions.Count);
            Assert.AreEqual(5, sentImpressions.FirstOrDefault(i => i.F.Equals("FACUNDO_TEST")).I.Count);
            Assert.AreEqual(3, sentImpressions.FirstOrDefault(i => i.F.Equals("MAURO_TEST")).I.Count);
            Assert.AreEqual(1, sentImpressions.FirstOrDefault(i => i.F.Equals("Test_Save_1")).I.Count);

            var impressionCounts = GetImpressionsCountsSentBackend(httpClientMock);
            Assert.AreEqual(0, impressionCounts.Count);

            ShutdownServer(httpClientMock);
        }

        #region Protected Methods
        protected override ConfigurationOptions GetConfigurationOptions(HttpClientMock httpClientMock = null, int? eventsPushRate = null, int? eventsQueueSize = null, int? featuresRefreshRate = null, bool? ipAddressesEnabled = null)
        {
            _impressionListener = new IntegrationTestsImpressionListener(50);

            return new ConfigurationOptions
            {
                Endpoint = $"http://localhost:{httpClientMock.GetPort()}",
                EventsEndpoint = $"http://localhost:{httpClientMock.GetPort()}",
                ReadTimeout = 20000,
                ConnectionTimeout = 20000,
                ImpressionListener = _impressionListener,
                FeaturesRefreshRate = featuresRefreshRate ?? 1,
                SegmentsRefreshRate = 1,
                ImpressionsRefreshRate = 1,
                MetricsRefreshRate = 1,
                EventsPushRate = eventsPushRate ?? 1,
                EventsQueueSize = eventsQueueSize,
                IPAddressesEnabled = ipAddressesEnabled
            };
        }

        protected override HttpClientMock GetHttpClientMock()
        {
            var httpClientMock = new HttpClientMock();
            httpClientMock.SplitChangesOk("split_changes.json", "-1");
            httpClientMock.SplitChangesOk("split_changes_1.json", "1506703262916");

            httpClientMock.SegmentChangesOk("-1", "segment1");
            httpClientMock.SegmentChangesOk("1470947453877", "segment1");

            httpClientMock.SegmentChangesOk("-1", "segment2");
            httpClientMock.SegmentChangesOk("1470947453878", "segment2");

            httpClientMock.SegmentChangesOk("-1", "segment3");
            httpClientMock.SegmentChangesOk("1470947453879", "segment3");

            return httpClientMock;
        }

        protected override void ShutdownServer(HttpClientMock httpClientMock = null)
        {
            httpClientMock.ShutdownServer();
        }

        protected override void AssertSentImpressions(int sentImpressionsCount, HttpClientMock httpClientMock = null, params KeyImpression[] expectedImpressions)
        {
            Thread.Sleep(1500);

            var sentImpressions = GetImpressionsSentBackend(httpClientMock);

            Assert.AreEqual(sentImpressionsCount, sentImpressions.Sum(si => si.I.Count));

            foreach (var expectedImp in expectedImpressions)
            {                
                var keyImpressions = sentImpressions.First(si => si.F.Equals(expectedImp.feature)).I;

                AssertImpression(expectedImp, keyImpressions);
            }
        }
        
        protected void AssertImpression(KeyImpression impressionExpected, List<ImpressionData> sentImpressions)
        {
            Assert.IsTrue(sentImpressions
                .Where(si => impressionExpected.bucketingKey == si.B)
                .Where(si => impressionExpected.changeNumber == si.C)
                .Where(si => impressionExpected.keyName == si.K)
                .Where(si => impressionExpected.label == si.R)
                .Where(si => impressionExpected.treatment == si.T)
                .Any());
        }

        protected override void AssertSentEvents(List<EventBackend> eventsExpected, HttpClientMock httpClientMock = null, int sleepTime = 15000, int? eventsCount = null, bool validateEvents = true)
        {
            Thread.Sleep(sleepTime);

            var sentEvents = GetEventsSentBackend(httpClientMock);

            Assert.AreEqual(eventsCount ?? eventsExpected.Count, sentEvents.Count);

            if (validateEvents)
            {
                foreach (var expected in eventsExpected)
                {
                    Assert.IsTrue(sentEvents
                        .Where(ee => ee.Key == expected.Key)
                        .Where(ee => ee.EventTypeId == expected.EventTypeId)
                        .Where(ee => ee.Value == expected.Value)
                        .Where(ee => ee.TrafficTypeName == expected.TrafficTypeName)
                        .Where(ee => ee.Properties?.Count == expected.Properties?.Count)
                        .Any());
                }
            }
        }
        #endregion

        #region Private Methods
        private List<KeyImpressionBackend> GetImpressionsSentBackend(HttpClientMock httpClientMock = null)
        {
            var impressions = new List<KeyImpressionBackend>();
            var logs = httpClientMock.GetImpressionLogs();

            foreach (var log in logs)
            {
                var _impressions = JsonConvert.DeserializeObject<List<KeyImpressionBackend>>(log.RequestMessage.Body);

                foreach (var _imp in _impressions)
                {
                    impressions.Add(_imp);
                }
            }

            return impressions;
        }

        private List<ImpressionCount> GetImpressionsCountsSentBackend(HttpClientMock httpClientMock = null)
        {
            var impressions = new List<ImpressionCount>();
            var logs = httpClientMock.GetImpressionCountsLogs();

            foreach (var log in logs)
            {
                var _impression = JsonConvert.DeserializeObject<ImpressionCount>(log.RequestMessage.Body);

                impressions.Add(_impression);

            }

            return impressions;
        }

        private List<EventBackend> GetEventsSentBackend(HttpClientMock httpClientMock = null)
        {
            var events = new List<EventBackend>();
            var logs = httpClientMock.GetEventsLog();

            foreach (var log in logs)
            {
                var _events = JsonConvert.DeserializeObject<List<EventBackend>>(log.RequestMessage.Body);

                foreach (var item in _events)
                {
                    events.Add(item);
                }
            }

            return events;
        }
        #endregion
    }
}
