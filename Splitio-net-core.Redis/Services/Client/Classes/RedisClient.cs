using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Redis.Services.Domain;
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
        private readonly RedisConfig _config;

        private IRedisAdapter _redisAdapter;
        private ISimpleCache<IList<KeyImpression>> _impressionsCacheRedis;        

        public RedisClient(ConfigurationOptions config,
            string apiKey,
            ISplitLogger log = null) : base(GetLogger(log))
        {
            _config = new RedisConfig();
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
            _config.SdkVersion = data.SdkVersion;
            _config.SdkSpecVersion = data.SdkSpecVersion;
            _config.SdkMachineName = data.SdkMachineName;
            _config.SdkMachineIP = data.SdkMachineIP;

            _config.RedisHost = config.CacheAdapterConfig.Host;
            _config.RedisPort = config.CacheAdapterConfig.Port;
            _config.RedisPassword = config.CacheAdapterConfig.Password;
            _config.RedisDatabase = config.CacheAdapterConfig.Database ?? 0;
            _config.RedisConnectTimeout = config.CacheAdapterConfig.ConnectTimeout ?? 0;
            _config.RedisSyncTimeout = config.CacheAdapterConfig.SyncTimeout ?? 0;
            _config.RedisConnectRetry = config.CacheAdapterConfig.ConnectRetry ?? 0;
            _config.RedisUserPrefix = config.CacheAdapterConfig.UserPrefix;
            LabelsEnabled = config.LabelsEnabled ?? true;
        }

        private void BuildRedisCache()
        {
            _redisAdapter = new RedisAdapter(_config.RedisHost, _config.RedisPort, _config.RedisPassword, _config.RedisDatabase, _config.RedisConnectTimeout, _config.RedisConnectRetry, _config.RedisSyncTimeout);

            Task.Factory.StartNew(() => _redisAdapter.Connect());

            _segmentCache = new RedisSegmentCache(_redisAdapter, _config.RedisUserPrefix);
            BuildParser();
            _splitCache = new RedisSplitCache(_redisAdapter, _splitParser, _config.RedisUserPrefix);
            
            _metricsCache = new RedisMetricsCache(_redisAdapter, _config.SdkMachineIP, _config.SdkVersion, _config.SdkMachineName, _config.RedisUserPrefix);
            _impressionsCacheRedis = new RedisImpressionsCache(_redisAdapter, _config.SdkMachineIP, _config.SdkVersion, _config.SdkMachineName, _config.RedisUserPrefix);
            _eventsCache = new RedisEventsCache(_redisAdapter, _config.SdkMachineName, _config.SdkMachineIP, _config.SdkVersion, _config.RedisUserPrefix);

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
