using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Events.Classes;
using Splitio.Redis.Services.Impressions.Classes;
using Splitio.Redis.Services.Metrics.Classes;
using Splitio.Redis.Services.Parsing.Classes;
using Splitio.Services.Client.Classes;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Splitio.Redis.Services.Client.Classes
{
    public class RedisClient : SplitClient
    {
        private RedisAdapter redisAdapter;
        private IListener<IList<KeyImpression>> impressionListenerRedis;
        private ISimpleCache<IList<KeyImpression>> impressionsCacheRedis;
        
        private static string SdkVersion;
        private static string SdkSpecVersion;
        private static string SdkMachineName;
        private static string SdkMachineIP;
        private static string RedisHost;
        private static string RedisPort;
        private static string RedisPassword;
        private static int RedisDatabase;
        private static int RedisConnectTimeout;
        private static int RedisConnectRetry;
        private static int RedisSyncTimeout;
        private static string RedisUserPrefix;
        private static int BlockMilisecondsUntilReady;

        public RedisClient(ConfigurationOptions config,
            string apiKey,
            ISplitLogger log = null) : base(GetLogger(log))
        {
            ApiKey = apiKey;

            ReadConfig(config);
            BuildRedisCache();
            BuildTreatmentLog(config);
            BuildEventLog(config);
            BuildMetricsLog();            
            BuildBlockUntilReadyService();
            BuildManager();
            BuildParser();
            BuildEvaluator();
        }

        #region Protected Methods
        protected override void ImpressionLog(List<KeyImpression> impressionsQueue)
        {
            base.ImpressionLog(impressionsQueue);

            if (impressionListenerRedis != null)
            {
                impressionListenerRedis.Log(impressionsQueue);
            }
        }
        #endregion

        #region Private Methods
        private void ReadConfig(ConfigurationOptions config)
        {
            var data = _wrapperAdapter.ReadConfig(config, _log);
            SdkVersion = data.SdkVersion;
            SdkSpecVersion = data.SdkSpecVersion;
            SdkMachineName = data.SdkMachineName;
            SdkMachineIP = data.SdkMachineIP;

            RedisHost = config.CacheAdapterConfig.Host;
            RedisPort = config.CacheAdapterConfig.Port;
            RedisPassword = config.CacheAdapterConfig.Password;
            RedisDatabase = config.CacheAdapterConfig.Database ?? 0;
            RedisConnectTimeout = config.CacheAdapterConfig.ConnectTimeout ?? 0;
            RedisSyncTimeout = config.CacheAdapterConfig.SyncTimeout ?? 0;
            RedisConnectRetry = config.CacheAdapterConfig.ConnectRetry ?? 0;
            RedisUserPrefix = config.CacheAdapterConfig.UserPrefix;
            LabelsEnabled = config.LabelsEnabled ?? true;
            BlockMilisecondsUntilReady = config.Ready ?? 0;
        }

        private void BuildRedisCache()
        {
            redisAdapter = new RedisAdapter(RedisHost, RedisPort, RedisPassword, RedisDatabase, RedisConnectTimeout, RedisConnectRetry, RedisSyncTimeout);

            if (BlockMilisecondsUntilReady > 0 && !redisAdapter.IsConnected())
            {
                throw new TimeoutException($"SDK was not ready in {BlockMilisecondsUntilReady} miliseconds. Could not connect to Redis");
            }

            segmentCache = new RedisSegmentCache(redisAdapter, RedisUserPrefix);
            BuildParser();
            splitCache = new RedisSplitCache(redisAdapter, _splitParser, RedisUserPrefix);
            
            metricsCache = new RedisMetricsCache(redisAdapter, SdkMachineIP, SdkVersion, RedisUserPrefix);
            impressionsCacheRedis = new RedisImpressionsCache(redisAdapter, SdkMachineIP, SdkVersion, RedisUserPrefix);
            eventsCache = new RedisEventsCache(redisAdapter, SdkMachineName, SdkMachineIP, SdkVersion, RedisUserPrefix);

            _trafficTypeValidator = new TrafficTypeValidator(splitCache);
        }

        private void BuildTreatmentLog(ConfigurationOptions config)
        {
            var treatmentLog = new RedisTreatmentLog(impressionsCacheRedis);
            impressionListenerRedis = new AsynchronousListener<IList<KeyImpression>>(WrapperAdapter.GetLogger("AsynchronousImpressionListener"));
            ((AsynchronousListener<IList<KeyImpression>>)impressionListenerRedis).AddListener(treatmentLog);

            if (config.ImpressionListener != null)
            {
                impressionListener = new AsynchronousListener<KeyImpression>(WrapperAdapter.GetLogger("AsynchronousImpressionListener"));
                ((AsynchronousListener<KeyImpression>)impressionListener).AddListener(config.ImpressionListener);
            }
        }

        private void BuildEventLog(ConfigurationOptions config)
        {
            var eventLog = new RedisEventLog(eventsCache);
            eventListener = new AsynchronousListener<WrappedEvent>(WrapperAdapter.GetLogger("AsynchronousEventListener"));
            ((AsynchronousListener<WrappedEvent>)eventListener).AddListener(eventLog);
        }

        private void BuildMetricsLog()
        {
            metricsLog = new RedisMetricsLog(metricsCache);
        }
        
        private void BuildManager()
        {
            manager = new SplitManager(splitCache, _blockUntilReadyService);
        }

        private void BuildParser()
        {
            _splitParser = new RedisSplitParser(segmentCache);
        }

        private void BuildBlockUntilReadyService()
        {
            _blockUntilReadyService = new NoopBlockUntilReadyService();
        }

        private static ISplitLogger GetLogger(ISplitLogger splitLogger = null)
        {
            return splitLogger ?? WrapperAdapter.GetLogger(typeof(RedisClient));
        }
        #endregion
    }
}
