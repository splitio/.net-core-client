using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.Metrics.Classes;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Splitio.Services.Client.Classes
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
                Log.Warn(e, "Exception retrieving machine name.");
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
                Log.Warn(e, "Exception retrieving machine IP.");
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
        }

        private void BuildTreatmentLog(ConfigurationOptions config)
        {
            var treatmentLog = new RedisTreatmentLog(impressionsCache);
            impressionListener = new AsynchronousImpressionListener();
            ((AsynchronousImpressionListener)impressionListener).AddListener(treatmentLog);
            if (config.ImpressionListener != null)
            {
                ((AsynchronousImpressionListener)impressionListener).AddListener(config.ImpressionListener);
            }
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

        protected override string GetTreatmentForFeature(Key key, string feature, Dictionary<string, object> attributes)
        {
            long start = CurrentTimeHelper.CurrentTimeMillis();
            var clock = new Stopwatch();
            clock.Start();

            try
            {
                var split = splitCache.GetSplit(feature);

                if (split == null)
                {
                    //if split definition was not found, impression label = "rules not found"
                    RecordStats(key, feature, null, LabelSplitNotFound, start, Control, SdkGetTreatment, clock);

                    Log.Warn(string.Format("Unknown or invalid feature: {0}", feature));
                    return Control;
                }

                ParsedSplit parsedSplit = splitParser.Parse((Split)split);

                var treatment = GetTreatment(key, parsedSplit, attributes, start, clock);

                return treatment;
            }
            catch (Exception e)
            {
                //if there was an exception, impression label = "exception"
                RecordStats(key, feature, null, LabelException, start, Control, SdkGetTreatment, clock);

                Log.Error(e, string.Format("Exception caught getting treatment for feature: {0}", feature));
                return Control;
            }
        }      
    }
}
