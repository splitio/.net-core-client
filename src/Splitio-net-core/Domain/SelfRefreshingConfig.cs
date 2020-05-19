namespace Splitio.Domain
{
    public class SelfRefreshingConfig : BaseConfig
    {
        public string BaseUrl { get; set; }
        public string EventsBaseUrl { get; set; }
        public int SplitsRefreshRate { get; set; }
        public int SegmentRefreshRate { get; set; }
        public long HttpConnectionTimeout { get; set; }
        public long HttpReadTimeout { get; set; }
        public int ConcurrencyLevel { get; set; }
        public int TreatmentLogRefreshRate { get; set; }
        public int TreatmentLogSize { get; set; }
        public int EventsFirstPushWindow { get; set; }
        public int EventLogRefreshRate { get; set; }
        public int EventLogSize { get; set; }
        public int MaxCountCalls { get; set; }
        public int MaxTimeBetweenCalls { get; set; }
        public int NumberOfParalellSegmentTasks { get; set; }
        public bool RandomizeRefreshRates { get; set; }
        public bool StreamingEnabled { get; set; }
        public int AuthRetryBackoffBase { get; set; }
        public int StreamingReconnectBackoffBase { get; set; }
        public string AuthServiceURL { get; set; }
        public string StreamingServiceURL { get; set; }
    }
}
