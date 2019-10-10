﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private const string UserPrefix = "";
        private const int Database = 0;

        private readonly IRedisAdapter _redisAdapter;
        private readonly string rootFilePath;

        public RedisTests()
        {
            _redisAdapter = new RedisAdapter(Host, Port, Password, Database);

            rootFilePath = string.Empty;

#if NETCORE
            rootFilePath = @"Resources\";
#endif
        }
        
        #region Protected Methods
        protected override ConfigurationOptions GetConfigurationOptions(HttpClientMock httpClientMock = null, int? eventsPushRate = null, int? eventsQueueSize = null, int? featuresRefreshRate = null)
        {
            _impressionListener = new IntegrationTestsImpressionListener(50);

            var cacheConfig = new CacheAdapterConfigurationOptions
            {
                Type = AdapterType.Redis,
                Host = Host,
                Port = Port,
                Password = Password,
                Database = Database,
                ConnectTimeout = 5000,
                ConnectRetry = 3,
                SyncTimeout = 1000,
                UserPrefix = UserPrefix
            };            

            return new ConfigurationOptions
            {
                ReadTimeout = 20000,
                ConnectionTimeout = 20000,
                ImpressionListener = _impressionListener,
                FeaturesRefreshRate = featuresRefreshRate ?? 1,
                SegmentsRefreshRate = 1,
                ImpressionsRefreshRate = 1,
                MetricsRefreshRate = 1,
                EventsPushRate = eventsPushRate ?? 1,
                Mode = Mode.Consumer,
                CacheAdapterConfig = cacheConfig
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

            var actualImpressions = new List<KeyImpressionRedis>();

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
