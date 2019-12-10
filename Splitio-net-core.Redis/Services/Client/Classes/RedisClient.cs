using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Redis.Services.Events.Classes;
using Splitio.Redis.Services.Impressions.Classes;
using Splitio.Redis.Services.Metrics.Classes;
using Splitio.Redis.Services.Parsing.Classes;
using Splitio.Redis.Services.Shared;
using Splitio.Services.Client.Classes;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Splitio.Redis.Services.Client.Classes
{
    public class RedisClient : SplitClient
    {
        private IRedisAdapter _redisAdapter;
        private ISimpleCache<IList<KeyImpression>> _impressionsCacheRedis;
        
        private string SdkVersion;
        private string SdkSpecVersion;
        private string SdkMachineName;
        private string SdkMachineIP;
        private string RedisHost;
        private string RedisPort;
        private string RedisPassword;
        private string RedisUserPrefix;
        private int RedisDatabase;
        private int RedisConnectTimeout;
        private int RedisConnectRetry;
        private int RedisSyncTimeout;        

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
        }

        private void BuildRedisCache()
        {
            _redisAdapter = new RedisAdapter(RedisHost, RedisPort, RedisPassword, RedisDatabase, RedisConnectTimeout, RedisConnectRetry, RedisSyncTimeout);

            Task.Factory.StartNew(() => _redisAdapter.Connect());

            _segmentCache = new RedisSegmentCache(_redisAdapter, RedisUserPrefix);
            BuildParser();
            _splitCache = new RedisSplitCache(_redisAdapter, _splitParser, RedisUserPrefix);
            
            _metricsCache = new RedisMetricsCache(_redisAdapter, SdkMachineIP, SdkVersion, SdkMachineName, RedisUserPrefix);
            _impressionsCacheRedis = new RedisImpressionsCache(_redisAdapter, SdkMachineIP, SdkVersion, SdkMachineName, RedisUserPrefix);
            _eventsCache = new RedisEventsCache(_redisAdapter, SdkMachineName, SdkMachineIP, SdkVersion, RedisUserPrefix);

            _trafficTypeValidator = new TrafficTypeValidator(_splitCache);
        }

        private void BuildTreatmentLog(ConfigurationOptions config)
        {
            var treatmentLog = new RedisTreatmentLog(_impressionsCacheRedis);
            _impressionListener = new AsynchronousListener<IList<KeyImpression>>(WrapperAdapter.GetLogger("AsynchronousImpressionListenerRedis"));
            _impressionListener.AddListener(treatmentLog);

            if (config.ImpressionListener != null)
            {                
                _impressionListener.AddListener(config.ImpressionListener);
            }
        }

        private void BuildEventLog(ConfigurationOptions config)
        {
            var eventLog = new RedisEventLog(_eventsCache);

            _eventListener = new AsynchronousListener<WrappedEvent>(WrapperAdapter.GetLogger("AsynchronousEventListenerRedis"));
            _eventListener.AddListener(eventLog);
        }

        private void BuildMetricsLog()
        {
            _metricsLog = new RedisMetricsLog(_metricsCache);
        }
        
        private void BuildManager()
        {
            _manager = new SplitManager(_splitCache, _blockUntilReadyService);
        }

        private void BuildParser()
        {
            _splitParser = new RedisSplitParser(_segmentCache);
        }

        private void BuildBlockUntilReadyService()
        {
            _blockUntilReadyService = new RedisBlockUntilReadyService(_redisAdapter);
        }

        private static ISplitLogger GetLogger(ISplitLogger splitLogger = null)
        {
            return splitLogger ?? WrapperAdapter.GetLogger(typeof(RedisClient));
        }
        #endregion
    }
}
