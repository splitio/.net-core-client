using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio_net_core.Integration_tests.Resources;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Splitio_net_core.Integration_tests
{
    [TestClass]
    public class RedisTests : BaseIntegrationTests
    {
        private const string Host = "localhost";
        private const string Port = "6379";
        private const string Password = "";
        private const int Database = 0;

        private readonly IRedisAdapter _redisAdapter;
        private readonly string rootFilePath;

        public RedisTests()
        {
            _redisAdapter = new RedisAdapter(Host, Port, Password, Database);
            _redisAdapter.Connect();

            rootFilePath = string.Empty;

#if NETCORE
            rootFilePath = @"Resources\";
#endif
        }

        [TestMethod]
        public void CheckingMachineIpAndMachineName_WithIPAddressesEnabled_ReturnsIpAndName()
        {
            // Arrange.
            GetHttpClientMock();
            var configurations = GetConfigurationOptions();

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var treatmentResult1 = client.GetTreatment("mauro_test", "FACUNDO_TEST");
            var treatmentResult2 = client.GetTreatment("nico_test", "FACUNDO_TEST");
            var treatmentResult3 = client.GetTreatment("redo_test", "FACUNDO_TEST");
            var trackResult1 = client.Track("mauro", "user", "event_type");
            var trackResult2 = client.Track("nicolas", "user_2", "event_type_2");
            var trackResult3 = client.Track("redo", "user_3", "event_type_3");

            // Assert.
            Thread.Sleep(1500);

            // Impressions
            var redisImpressions = _redisAdapter.ListRange("SPLITIO.impressions");

            foreach (var item in redisImpressions)
            {
                var impression = JsonConvert.DeserializeObject<KeyImpressionRedis>(item);

                Assert.AreNotEqual("NA", impression.M.I);
                Assert.AreNotEqual("NA", impression.M.N);
            }

            // Events 
            var sdkVersion = string.Empty;
            var redisEvents = _redisAdapter.ListRange("SPLITIO.events");

            foreach (var item in redisEvents)
            {
                var eventRedis = JsonConvert.DeserializeObject<EventRedis>(item);

                Assert.AreNotEqual("NA", eventRedis.M.I);
                Assert.AreNotEqual("NA", eventRedis.M.N);

                sdkVersion = eventRedis.M.S;
            }

            // Metrics
            var keys = _redisAdapter.Keys($"SPLITIO/{sdkVersion}/*");

            foreach (var key in keys)
            {
                Assert.IsFalse(key.ToString().Contains("/NA/"));
            }

            ShutdownServer();
        }

        [TestMethod]
        public void CheckingMachineIpAndMachineName_WithIPAddressesDisabled_ReturnsNA()
        {
            // Arrange.           
            GetHttpClientMock();
            var configurations = GetConfigurationOptions(ipAddressesEnabled: false);

            var apikey = "apikey1";

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);

            // Act.
            var treatmentResult1 = client.GetTreatment("mauro_test", "FACUNDO_TEST");
            var treatmentResult2 = client.GetTreatment("nico_test", "FACUNDO_TEST");
            var treatmentResult3 = client.GetTreatment("redo_test", "FACUNDO_TEST");
            var trackResult1 = client.Track("mauro", "user", "event_type");
            var trackResult2 = client.Track("nicolas", "user_2", "event_type_2");
            var trackResult3 = client.Track("redo", "user_3", "event_type_3");

            // Assert.
            Thread.Sleep(1500);

            // Impressions
            var redisImpressions = _redisAdapter.ListRange("SPLITIO.impressions");

            foreach (var item in redisImpressions)
            {
                var impression = JsonConvert.DeserializeObject<KeyImpressionRedis>(item);

                Assert.AreEqual("NA", impression.M.I);
                Assert.AreEqual("NA", impression.M.N);
            }

            // Events 
            var sdkVersion = string.Empty;
            var redisEvents = _redisAdapter.ListRange("SPLITIO.events");

            foreach (var item in redisEvents)
            {
                var eventRedis = JsonConvert.DeserializeObject<EventRedis>(item);

                Assert.AreEqual("NA", eventRedis.M.I);
                Assert.AreEqual("NA", eventRedis.M.N);

                sdkVersion = eventRedis.M.S;
            }

            // Metrics
            var keys = _redisAdapter.Keys($"SPLITIO/{sdkVersion}/*");

            foreach (var key in keys)
            {
                Assert.IsTrue(key.ToString().Contains("/NA/"));
            }

            ShutdownServer();
        }

        #region Protected Methods
        protected override ConfigurationOptions GetConfigurationOptions(HttpClientMock httpClientMock = null, int? eventsPushRate = null, int? eventsQueueSize = null, int? featuresRefreshRate = null, bool? ipAddressesEnabled = null)
        {
            _impressionListener = new IntegrationTestsImpressionListener(50);

            var cacheConfig = new CacheAdapterConfigurationOptions
            {
                Host = Host,
                Port = Port,
                Password = Password,
                Database = Database                
            };            

            return new ConfigurationOptions
            {
                ImpressionListener = _impressionListener,
                FeaturesRefreshRate = featuresRefreshRate ?? 1,
                SegmentsRefreshRate = 1,
                ImpressionsRefreshRate = 1,
                MetricsRefreshRate = 1,
                EventsPushRate = eventsPushRate ?? 1,                
                IPAddressesEnabled = ipAddressesEnabled,
                CacheAdapterConfig = cacheConfig,
                Mode = Mode.Consumer
            };
        }

        protected override HttpClientMock GetHttpClientMock()
        {
            LoadSplits();

            return null;
        }

        protected override void ShutdownServer(HttpClientMock httpClientMock = null)
        {
            _redisAdapter.Flush();
        }

        protected override void AssertSentImpressions(int sentImpressionsCount, HttpClientMock httpClientMock = null, params KeyImpression[] expectedImpressions)
        {
            Thread.Sleep(1500);

            var redisImpressions = _redisAdapter.ListRange("SPLITIO.impressions");

            Assert.AreEqual(sentImpressionsCount, redisImpressions.Length);

            foreach (var item in redisImpressions)
            {
                var actualImp = JsonConvert.DeserializeObject<KeyImpressionRedis>(item);

                AssertImpression(actualImp, expectedImpressions.ToList());
            }
        }

        protected override void AssertSentEvents(List<EventBackend> eventsExcpected, HttpClientMock httpClientMock = null, int sleepTime = 15000, int? eventsCount = null, bool validateEvents = true)
        {
            Thread.Sleep(sleepTime);

            var redisEvents = _redisAdapter.ListRange("SPLITIO.events");

            Assert.AreEqual(eventsExcpected.Count, redisEvents.Length);

            foreach (var item in redisEvents)
            {
                var actualEvent = JsonConvert.DeserializeObject<EventRedis>(item);

                AssertEvent(actualEvent, eventsExcpected);
            }
        }
        #endregion

        #region Private Methods        
        private void AssertImpression(KeyImpressionRedis impressionActual, List<KeyImpression> sentImpressions)
        {
            Assert.IsFalse(string.IsNullOrEmpty(impressionActual.M.I));
            Assert.IsFalse(string.IsNullOrEmpty(impressionActual.M.N));
            Assert.IsFalse(string.IsNullOrEmpty(impressionActual.M.S));

            Assert.IsTrue(sentImpressions
                .Where(si => impressionActual.I.B == si.bucketingKey)
                .Where(si => impressionActual.I.C == si.changeNumber)
                .Where(si => impressionActual.I.K == si.keyName)
                .Where(si => impressionActual.I.R == si.label)
                .Where(si => impressionActual.I.T == si.treatment)
                .Any());
        }

        private void AssertEvent(EventRedis eventActual, List<EventBackend> eventsExcpected)
        {
            Assert.IsFalse(string.IsNullOrEmpty(eventActual.M.I));
            Assert.IsFalse(string.IsNullOrEmpty(eventActual.M.N));
            Assert.IsFalse(string.IsNullOrEmpty(eventActual.M.S));

            Assert.IsTrue(eventsExcpected
                .Where(ee => eventActual.E.EventTypeId == ee.EventTypeId)
                .Where(ee => eventActual.E.Key == ee.Key)
                .Where(ee => eventActual.E.Properties?.Count == ee.Properties?.Count)
                .Where(ee => eventActual.E.TrafficTypeName == ee.TrafficTypeName)
                .Where(ee => eventActual.E.Value == ee.Value)
                .Any());
        }

        private void LoadSplits()
        {
            _redisAdapter.Flush();

            var splitsJson = File.ReadAllText($"{rootFilePath}split_changes.json");
            var segmentJson1 = File.ReadAllText($"{rootFilePath}split_segment1.json");
            var segmentJson2 = File.ReadAllText($"{rootFilePath}split_segment2.json");
            var segmentJson3 = File.ReadAllText($"{rootFilePath}split_segment3.json");

            var splitResult = JsonConvert.DeserializeObject<SplitChangesResult>(splitsJson);

            foreach (var split in splitResult.splits)
            {
                _redisAdapter.Set($"SPLITIO.split.{split.name}", JsonConvert.SerializeObject(split));
            }
        }
        #endregion
    }
}
