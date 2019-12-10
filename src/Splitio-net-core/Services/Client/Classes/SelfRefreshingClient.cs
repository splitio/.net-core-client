using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Events.Classes;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Classes;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.Parsing.Classes;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using Splitio.Services.SplitFetcher.Classes;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Splitio.Services.Client.Classes
{
    public class SelfRefreshingClient : SplitClient
    {
        private static string BaseUrl;
        private static int SplitsRefreshRate;
        private static int SegmentRefreshRate;
        private static long HttpConnectionTimeout;
        private static long HttpReadTimeout;
        private static string SdkVersion;
        private static string SdkSpecVersion;
        private static string SdkMachineName;
        private static string SdkMachineIP;
        private static bool RandomizeRefreshRates;
        private static int ConcurrencyLevel;
        private static int TreatmentLogRefreshRate;
        private static int TreatmentLogSize;
        private static int EventsFirstPushWindow;
        private static int EventLogRefreshRate;
        private static int EventLogSize;
        private static string EventsBaseUrl;
        private static int MaxCountCalls;
        private static int MaxTimeBetweenCalls;
        private static int NumberOfParalellSegmentTasks;

        /// <summary>
        /// Represents the initial number of buckets for a ConcurrentDictionary. 
        /// Should not be divisible by a small prime number. 
        /// The default capacity is 31. 
        /// More details : https://msdn.microsoft.com/en-us/library/dd287171(v=vs.110).aspx
        /// </summary>
        private const int InitialCapacity = 31;

        private IReadinessGatesCache _gates;
        private ISplitFetcher _splitFetcher;
        private ISplitSdkApiClient _splitSdkApiClient;
        private ISegmentSdkApiClient _segmentSdkApiClient;
        private ITreatmentSdkApiClient _treatmentSdkApiClient;
        private IEventSdkApiClient _eventSdkApiClient;
        private IMetricsSdkApiClient _metricsSdkApiClient;
        private ISplitFetcher _selfRefreshingSegmentFetcher;
        private IListener<IList<KeyImpression>> _treatmentLog;
        private IListener<WrappedEvent> _eventLog;

        public SelfRefreshingClient(string apiKey, 
            ConfigurationOptions config, 
            ISplitLogger log = null) : base(GetLogger(log))
        {
            Destroyed = false;

            ApiKey = apiKey;
            ReadConfig(config);
            BuildSdkReadinessGates();
            BuildSdkApiClients();
            BuildSplitFetcher();
            BuildTreatmentLog(config);
            BuildEventLog(config);
            BuildEvaluator();
            BuildBlockUntilReadyService();
            BuildManager();

            Start();
            LaunchTaskSchedulerOnReady();
        }

        #region Public Methods
        public void Start()
        {
            ((SelfUpdatingTreatmentLog)_treatmentLog).Start();
            ((SelfUpdatingEventLog)_eventLog).Start();
            _splitFetcher.Start();
        }

        public void Stop()
        {
            _splitFetcher.Stop(); // Stop + Clear
            _selfRefreshingSegmentFetcher.Stop(); // Stop + Clear
            ((SelfUpdatingTreatmentLog)_treatmentLog).Stop(); //Stop + SendBulk + Clear
            ((SelfUpdatingEventLog)_eventLog).Stop(); //Stop + SendBulk + Clear
            _metricsLog.Clear(); //Clear
        }

        public override void Destroy()
        {
            if (!Destroyed)
            {
                Stop();
                base.Destroy();
            }
        }
        #endregion

        #region Private Methods
        private void ReadConfig(ConfigurationOptions config)
        {
            BaseUrl = string.IsNullOrEmpty(config.Endpoint) ? "https://sdk.split.io" : config.Endpoint;
            EventsBaseUrl = string.IsNullOrEmpty(config.EventsEndpoint) ? "https://events.split.io" : config.EventsEndpoint;
            SplitsRefreshRate = config.FeaturesRefreshRate ?? 5;
            SegmentRefreshRate = config.SegmentsRefreshRate ?? 60;
            HttpConnectionTimeout = config.ConnectionTimeout ?? 15000;
            HttpReadTimeout = config.ReadTimeout ?? 15000;

            var data = _wrapperAdapter.ReadConfig(config, _log);
            SdkVersion = data.SdkVersion;
            SdkSpecVersion = data.SdkSpecVersion;
            SdkMachineName = data.SdkMachineName;
            SdkMachineIP = data.SdkMachineIP;

            RandomizeRefreshRates = config.RandomizeRefreshRates;
            ConcurrencyLevel = config.SplitsStorageConcurrencyLevel ?? 4;
            TreatmentLogRefreshRate = config.ImpressionsRefreshRate ?? 30;
            TreatmentLogSize = config.MaxImpressionsLogSize ?? 30000;
            EventLogRefreshRate = config.EventsPushRate ?? 60;
            EventLogSize = config.EventsQueueSize ?? 5000;
            EventsFirstPushWindow = config.EventsFirstPushWindow ?? 10;
            MaxCountCalls = config.MaxMetricsCountCallsBeforeFlush ?? 1000;
            MaxTimeBetweenCalls = config.MetricsRefreshRate ?? 60;
            NumberOfParalellSegmentTasks = config.NumberOfParalellSegmentTasks ?? 5;
            LabelsEnabled = config.LabelsEnabled ?? true;
        }

        private void BuildSdkReadinessGates()
        {
            _gates = new InMemoryReadinessGatesCache();
        }

        private void BuildSplitFetcher()
        {
            var segmentRefreshRate = RandomizeRefreshRates ? Random(SegmentRefreshRate) : SegmentRefreshRate;
            var splitsRefreshRate = RandomizeRefreshRates ? Random(SplitsRefreshRate) : SplitsRefreshRate;

            _segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>(ConcurrencyLevel, InitialCapacity));

            var segmentChangeFetcher = new ApiSegmentChangeFetcher(_segmentSdkApiClient);
            _selfRefreshingSegmentFetcher = new SelfRefreshingSegmentFetcher(segmentChangeFetcher, _gates, segmentRefreshRate, _segmentCache, NumberOfParalellSegmentTasks);

            var splitChangeFetcher = new ApiSplitChangeFetcher(_splitSdkApiClient);
            _splitParser = new InMemorySplitParser((SelfRefreshingSegmentFetcher)_selfRefreshingSegmentFetcher, _segmentCache);
            _splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>(ConcurrencyLevel, InitialCapacity));
            _splitFetcher = new SelfRefreshingSplitFetcher(splitChangeFetcher, _splitParser, _gates, splitsRefreshRate, _splitCache);

            _trafficTypeValidator = new TrafficTypeValidator(_splitCache);
        }

        private void BuildTreatmentLog(ConfigurationOptions config)
        {
            _impressionsCache = new InMemorySimpleCache<KeyImpression>(new BlockingQueue<KeyImpression>(TreatmentLogSize));
            _treatmentLog = new SelfUpdatingTreatmentLog(_treatmentSdkApiClient, TreatmentLogRefreshRate, _impressionsCache);
            _impressionListener = new AsynchronousListener<IList<KeyImpression>>(WrapperAdapter.GetLogger("AsynchronousImpressionListener"));
            _impressionListener.AddListener(_treatmentLog);

            if (config.ImpressionListener != null)
            {
                _impressionListener.AddListener(config.ImpressionListener);
            }
        }

        private void BuildEventLog(ConfigurationOptions config)
        {
            _eventsCache = new InMemorySimpleCache<WrappedEvent>(new BlockingQueue<WrappedEvent>(EventLogSize));
            _eventLog = new SelfUpdatingEventLog(_eventSdkApiClient, EventsFirstPushWindow, EventLogRefreshRate, _eventsCache);
            _eventListener = new AsynchronousListener<WrappedEvent>(WrapperAdapter.GetLogger("AsynchronousEventListener"));
            _eventListener.AddListener(_eventLog);
        }
        
        private void BuildMetricsLog()
        {
            _metricsCache = new InMemoryMetricsCache(new ConcurrentDictionary<string, Counter>(), new ConcurrentDictionary<string, ILatencyTracker>(), new ConcurrentDictionary<string, long>());
            _metricsLog = new AsyncMetricsLog(_metricsSdkApiClient, _metricsCache, MaxCountCalls, MaxTimeBetweenCalls);
        }

        private int Random(int refreshRate)
        {
            Random random = new Random();
            return Math.Max(5, random.Next(refreshRate / 2, refreshRate));
        }

        private void BuildSdkApiClients()
        {
            var header = new HTTPHeader
            {
                authorizationApiKey = ApiKey,
                splitSDKVersion = SdkVersion,
                splitSDKSpecVersion = SdkSpecVersion,
                splitSDKMachineName = SdkMachineName,
                splitSDKMachineIP = SdkMachineIP
            };

            _metricsSdkApiClient = new MetricsSdkApiClient(header, EventsBaseUrl, HttpConnectionTimeout, HttpReadTimeout);
            BuildMetricsLog();
            _splitSdkApiClient = new SplitSdkApiClient(header, BaseUrl, HttpConnectionTimeout, HttpReadTimeout, _metricsLog);
            _segmentSdkApiClient = new SegmentSdkApiClient(header, BaseUrl, HttpConnectionTimeout, HttpReadTimeout, _metricsLog);
            _treatmentSdkApiClient = new TreatmentSdkApiClient(header, EventsBaseUrl, HttpConnectionTimeout, HttpReadTimeout);
            _eventSdkApiClient = new EventSdkApiClient(header, EventsBaseUrl, HttpConnectionTimeout, HttpReadTimeout);
        }

        private void BuildManager()
        {
            _manager = new SplitManager(_splitCache, _blockUntilReadyService);
        }

        private void BuildBlockUntilReadyService()
        {
            _blockUntilReadyService = new SelfRefreshingBlockUntilReadyService(_gates, _log);
        }

        private void LaunchTaskSchedulerOnReady()
        {
            var workerTask = Task.Factory.StartNew(
                () => {
                    while (true)
                    {
                        if (_gates.IsSDKReady(0))
                        {
                            _selfRefreshingSegmentFetcher.Start();
                            break;
                        }

                        _wrapperAdapter.TaskDelay(500).Wait();
                    }
                });
        }

        private static ISplitLogger GetLogger(ISplitLogger splitLogger = null)
        {
            return splitLogger ?? WrapperAdapter.GetLogger(typeof(SelfRefreshingClient));
        }
        #endregion
    }
}
