using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;
using System;

namespace Splitio.Services.Shared.Classes
{
    public class ConfigService : IConfigService
    {
        private readonly IWrapperAdapter _wrapperAdapter;
        private readonly ISplitLogger _log;

        public ConfigService(IWrapperAdapter wrapperAdapter,
            ISplitLogger log)
        {
            _wrapperAdapter = wrapperAdapter;
            _log = log;
        }

        public BaseConfig ReadConfig(ConfigurationOptions config, ConfingTypes configType)
        {
            switch (configType)
            {
                case ConfingTypes.Redis:
                    return ReadBaseConfig(config);
                case ConfingTypes.InMemory:
                default:
                    return ReadInMemoryConfig(config);
            }
        }

        public BaseConfig ReadBaseConfig(ConfigurationOptions config)
        {
            var data = _wrapperAdapter.ReadConfig(config, _log);

            return new BaseConfig
            {
                SdkVersion = data.SdkVersion,
                SdkSpecVersion = data.SdkSpecVersion,
                SdkMachineName = data.SdkMachineName,
                SdkMachineIP = data.SdkMachineIP,
                LabelsEnabled = config.LabelsEnabled ?? true
            };
        }

        public SelfRefreshingConfig ReadInMemoryConfig(ConfigurationOptions config)
        {
            var baseConfig = ReadBaseConfig(config);

            var selfRefreshingConfig = new SelfRefreshingConfig
            {
                SdkVersion = baseConfig.SdkVersion,
                SdkSpecVersion = baseConfig.SdkSpecVersion,
                SdkMachineName = baseConfig.SdkMachineName,
                SdkMachineIP = baseConfig.SdkMachineIP,
                LabelsEnabled = baseConfig.LabelsEnabled,
                BaseUrl = string.IsNullOrEmpty(config.Endpoint) ? "https://sdk.split.io" : config.Endpoint,
                EventsBaseUrl = string.IsNullOrEmpty(config.EventsEndpoint) ? "https://events.split.io" : config.EventsEndpoint,
                SplitsRefreshRate = config.FeaturesRefreshRate ?? 5,
                SegmentRefreshRate = config.SegmentsRefreshRate ?? 60,
                HttpConnectionTimeout = config.ConnectionTimeout ?? 15000,
                HttpReadTimeout = config.ReadTimeout ?? 15000,                
                RandomizeRefreshRates = config.RandomizeRefreshRates,ConcurrencyLevel = config.SplitsStorageConcurrencyLevel ?? 4,
                TreatmentLogSize = config.MaxImpressionsLogSize ?? 30000,
                EventLogRefreshRate = config.EventsPushRate ?? 60,
                EventLogSize = config.EventsQueueSize ?? 5000,
                EventsFirstPushWindow = config.EventsFirstPushWindow ?? 10,
                MaxCountCalls = config.MaxMetricsCountCallsBeforeFlush ?? 1000,
                MaxTimeBetweenCalls = config.MetricsRefreshRate ?? 60,
                NumberOfParalellSegmentTasks = config.NumberOfParalellSegmentTasks ?? 5,
                StreamingEnabled = config.StreamingEnabled ?? true,
                AuthRetryBackoffBase = GetMinimunAllowed(config.AuthRetryBackoffBase ?? 1, 1, "AuthRetryBackoffBase"),
                StreamingReconnectBackoffBase = GetMinimunAllowed(config.StreamingReconnectBackoffBase ?? 1, 1, "StreamingReconnectBackoffBase"),
                AuthServiceURL = string.IsNullOrEmpty(config.AuthServiceURL) ? "https://auth.split.io/api/auth" : config.AuthServiceURL,
                StreamingServiceURL = string.IsNullOrEmpty(config.StreamingServiceURL) ? "https://streaming.split.io/event-stream" : config.StreamingServiceURL,
                ImpressionsMode = config.ImpressionsMode ?? ImpressionsMode.Optimized
            };

            selfRefreshingConfig.TreatmentLogRefreshRate = GetImpressionRefreshRate(selfRefreshingConfig.ImpressionsMode, config.ImpressionsRefreshRate);

            return selfRefreshingConfig;
        }

        private int GetMinimunAllowed(int value, int minAllowed, string configName)
        {
            if (value < minAllowed)
            {
                _log.Warn($"{configName} minimum allowed value: {minAllowed}");

                return minAllowed;
            }

            return value;
        }

        private int GetImpressionRefreshRate(ImpressionsMode impressionsMode, int? impressionsRefreshRate)
        {
            switch (impressionsMode)
            {
                case ImpressionsMode.Debug:
                    return impressionsRefreshRate == null || impressionsRefreshRate <= 0 ? 60 : impressionsRefreshRate.Value;
                case ImpressionsMode.Optimized:
                default:
                    return impressionsRefreshRate == null || impressionsRefreshRate <= 0 ? 300 : Math.Max(60, impressionsRefreshRate.Value);
            }
        }
    }

    public enum ConfingTypes
    {
        InMemory,
        Redis
    }
}
