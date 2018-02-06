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


        public RedisClient(ConfigurationOptions config)
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
                Log.Warn("Exception retrieving machine name.", e);
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
                Log.Warn("Exception retrieving machine IP.", e);
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
        }

        private void BuildRedisCache()
        {
            redisAdapter = new RedisAdapter(RedisHost, RedisPort, RedisPassword, RedisDatabase, RedisConnectTimeout, RedisConnectRetry, RedisSyncTimeout);
            splitCache = new RedisSplitCache(redisAdapter, RedisUserPrefix);
            segmentCache = new RedisSegmentCache(redisAdapter, RedisUserPrefix);
            metricsCache = new RedisMetricsCache(redisAdapter, SdkMachineIP, SdkVersion, RedisUserPrefix);
            impressionsCache = new RedisImpressionsCache(redisAdapter, SdkMachineIP, SdkVersion, RedisUserPrefix);
            eventsCache = new RedisEventsCache(redisAdapter, SdkMachineName, SdkMachineIP, SdkVersion, RedisUserPrefix);
        }

        private void BuildTreatmentLog(ConfigurationOptions config)
        {
            var treatmentLog = new RedisTreatmentLog(impressionsCache);
            impressionListener = new AsynchronousListener<KeyImpression>(LogManager.GetLogger("AsynchronousImpressionListener"));
            ((AsynchronousListener<KeyImpression>)impressionListener).AddListener(treatmentLog);
            if (config.ImpressionListener != null)
            {
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

        protected override string GetTreatmentForFeature(Key key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true)
        {
            long start = CurrentTimeHelper.CurrentTimeMillis();
            var clock = new Stopwatch();
            clock.Start();

            try
            {
                var split = splitCache.GetSplit(feature);

                if (split == null)
                {
                    if (logMetricsAndImpressions)
                    {
                        //if split definition was not found, impression label = "definition not found"
                        RecordStats(key, feature, null, LabelSplitNotFound, start, Control, SdkGetTreatment, clock);
                    }

                    Log.Warn(string.Format("Unknown or invalid feature: {0}", feature));
                    return Control;
                }

                ParsedSplit parsedSplit = splitParser.Parse((Split)split);

                var treatment = GetTreatment(key, parsedSplit, attributes, start, clock, this, logMetricsAndImpressions);

                return treatment;
            }
            catch (Exception e)
            {
                //if there was an exception, impression label = "exception"
                RecordStats(key, feature, null, LabelException, start, Control, SdkGetTreatment, clock);

                Log.Error(string.Format("Exception caught getting treatment for feature: {0}", feature), e);
                return Control;
            }
        }

        public override void Destroy()
        {
            return;
        }
    }
}
