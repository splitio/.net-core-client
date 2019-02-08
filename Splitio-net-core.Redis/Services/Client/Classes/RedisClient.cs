using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Events.Classes;
using Splitio.Redis.Services.Impressions.Classes;
using Splitio.Redis.Services.Metrics.Classes;
using Splitio.Redis.Services.Parsing.Classes;
using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Splitio.Redis.Services.Client.Classes
{
    public class RedisClient : SplitClient
    {
        private RedisSplitParser splitParser;
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

        public RedisClient(ConfigurationOptions config, ILog log) : base(log)
        {
            ReadConfig(config);
            BuildRedisCache();
            BuildTreatmentLog(config);
            BuildEventLog(config);
            BuildMetricsLog();
            BuildSplitter();
            BuildManager();
            BuildParser();
        }

        private void ReadConfig(ConfigurationOptions config)
        {
            SdkVersion = ".NET_CORE-" + Version.SplitSdkVersion;
            SdkSpecVersion = ".NET-" + Version.SplitSpecVersion;

            try
            {
                SdkMachineName = config.SdkMachineName ?? Environment.MachineName;
            }
            catch (Exception e)
            {
                SdkMachineName = "unknown";
                _log.Warn("Exception retrieving machine name.", e);
            }

            try
            {
                var hostAddressesTask = Dns.GetHostAddressesAsync(Environment.MachineName);
                hostAddressesTask.Wait();
                SdkMachineIP = config.SdkMachineIP ?? hostAddressesTask.Result.Where(x => x.AddressFamily == AddressFamily.InterNetwork && x.IsIPv6LinkLocal == false).Last().ToString();
            }
            catch (Exception e)
            {
                SdkMachineIP = "unknown";
                _log.Warn("Exception retrieving machine IP.", e);
            }

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

            splitCache = new RedisSplitCache(redisAdapter, RedisUserPrefix);
            segmentCache = new RedisSegmentCache(redisAdapter, RedisUserPrefix);
            metricsCache = new RedisMetricsCache(redisAdapter, SdkMachineIP, SdkVersion, RedisUserPrefix);
            impressionsCacheRedis = new RedisImpressionsCache(redisAdapter, SdkMachineIP, SdkVersion, RedisUserPrefix);
            eventsCache = new RedisEventsCache(redisAdapter, SdkMachineName, SdkMachineIP, SdkVersion, RedisUserPrefix);
        }

        private void BuildTreatmentLog(ConfigurationOptions config)
        {
            var treatmentLog = new RedisTreatmentLog(impressionsCacheRedis);
            impressionListenerRedis = new AsynchronousListener<IList<KeyImpression>>(LogManager.GetLogger("AsynchronousImpressionListener"));
            ((AsynchronousListener<IList<KeyImpression>>)impressionListenerRedis).AddListener(treatmentLog);

            if (config.ImpressionListener != null)
            {
                impressionListener = new AsynchronousListener<KeyImpression>(LogManager.GetLogger("AsynchronousImpressionListener"));
                ((AsynchronousListener<KeyImpression>)impressionListener).AddListener(config.ImpressionListener);
            }
        }

        private void BuildEventLog(ConfigurationOptions config)
        {
            var eventLog = new RedisEventLog(eventsCache);
            eventListener = new AsynchronousListener<Event>(LogManager.GetLogger("AsynchronousEventListener"));
            ((AsynchronousListener<Event>)eventListener).AddListener(eventLog);
        }

        private void BuildMetricsLog()
        {
            metricsLog = new RedisMetricsLog(metricsCache);
        }

        private void BuildSplitter()
        {
            splitter = new Splitter();
        }

        private void BuildManager()
        {
            manager = new RedisSplitManager(splitCache);
        }

        private void BuildParser()
        {
            splitParser = new RedisSplitParser(segmentCache);
        }

        protected override TreatmentResult GetTreatmentForFeature(Key key, string feature, Dictionary<string, object> attributes = null)
        {
            try
            {
                var split = splitCache.GetSplit(feature);

                if (split == null)
                {

                    _log.Warn(string.Format("Unknown or invalid feature: {0}", feature));

                    return new TreatmentResult(LabelSplitNotFound, Control, null);
                }

                ParsedSplit parsedSplit = splitParser.Parse((Split)split);

                return GetTreatment(key, parsedSplit, attributes, this);
            }
            catch (Exception e)
            {
                _log.Error(string.Format("Exception caught getting treatment for feature: {0}", feature), e);

                return new TreatmentResult(LabelException, Control, null);
            }
        }

        protected override void ImpressionLog<T>(IListener<T> listener, IList<T> impressions)
        {
            if (impressionListener != null)
            {
                base.ImpressionLog(impressionListener, ImpressionsQueue);
            }
            else if (impressionListenerRedis != null)
            {
                impressionListenerRedis.Log(ImpressionsQueue);
            }
        }

        public override void Destroy()
        {
            return;
        }
    }
}
