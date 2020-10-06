using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;

namespace Splitio.Services.Client.Classes
{
    public class ConfigurationOptions
    {
        public Mode Mode { get; set; }
        public string Endpoint { get; set; }
        public string EventsEndpoint { get; set; }
        public string LocalhostFilePath { get; set; }
        public int? FeaturesRefreshRate { get; set; } 
        public int? SegmentsRefreshRate { get; set; } 
        public bool RandomizeRefreshRates { get; set; } 
        public int? ImpressionsRefreshRate { get; set; }
        public int? MaxImpressionsLogSize { get; set; }
        public int? EventsFirstPushWindow { get; set; }
        public int? EventsPushRate { get; set; }
        public int? EventsQueueSize { get; set; }
        public long? ConnectionTimeout { get; set; } 
        public long? ReadTimeout { get; set; } 
        public int? MaxMetricsCountCallsBeforeFlush { get; set; } 
        public int? MetricsRefreshRate { get; set; } 
        public int? SplitsStorageConcurrencyLevel { get; set; }
        public string SdkMachineName { get; set; }
        public string SdkMachineIP { get; set; }
        public int? NumberOfParalellSegmentTasks { get; set; }
        public bool? LabelsEnabled { get; set; }
        public IImpressionListener ImpressionListener { get; set; }
        public CacheAdapterConfigurationOptions CacheAdapterConfig { get; set; }
        public bool? IPAddressesEnabled { get; set; }
        public bool? StreamingEnabled { get; set; }
        public int? AuthRetryBackoffBase { get; set; }
        public int? StreamingReconnectBackoffBase { get; set; }
        public string AuthServiceURL { get; set; }
        public string StreamingServiceURL { get; set; }
        public ImpressionsMode? ImpressionsMode { get; set; }
    }
}
