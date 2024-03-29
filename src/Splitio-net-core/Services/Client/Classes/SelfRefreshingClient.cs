﻿using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.Events.Classes;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.EventSource;
using Splitio.Services.EventSource.Workers;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Classes;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.Parsing.Classes;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.SplitFetcher.Classes;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Collections.Concurrent;

namespace Splitio.Services.Client.Classes
{
    public class SelfRefreshingClient : SplitClient
    {
        private readonly SelfRefreshingConfig _config;

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
        private ISelfRefreshingSegmentFetcher _selfRefreshingSegmentFetcher;
        private ISyncManager _syncManager;
        private IImpressionsCounter _impressionsCounter;

        public SelfRefreshingClient(string apiKey, 
            ConfigurationOptions config, 
            ISplitLogger log = null) : base(GetLogger(log))
        {
            _config = (SelfRefreshingConfig)_configService.ReadConfig(config, ConfingTypes.InMemory);
            LabelsEnabled = _config.LabelsEnabled;
            Destroyed = false;
            
            ApiKey = apiKey;
            BuildSdkReadinessGates();
            BuildSdkApiClients();
            BuildSplitFetcher();
            BuildTreatmentLog(config);
            BuildImpressionManager();
            BuildEventLog(config);
            BuildEvaluator();
            BuildBlockUntilReadyService();
            BuildManager();
            BuildSyncManager();

            Start();
        }

        #region Public Methods
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
        private void BuildSdkReadinessGates()
        {
            _gates = new InMemoryReadinessGatesCache();
        }

        private void BuildSplitFetcher()
        {
            var segmentRefreshRate = _config.RandomizeRefreshRates ? Random(_config.SegmentRefreshRate) : _config.SegmentRefreshRate;
            var splitsRefreshRate = _config.RandomizeRefreshRates ? Random(_config.SplitsRefreshRate) : _config.SplitsRefreshRate;

            _segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>(_config.ConcurrencyLevel, InitialCapacity));

            var segmentChangeFetcher = new ApiSegmentChangeFetcher(_segmentSdkApiClient);
            var segmentTaskQueue = new SegmentTaskQueue();
            _selfRefreshingSegmentFetcher = new SelfRefreshingSegmentFetcher(segmentChangeFetcher, _gates, segmentRefreshRate, _segmentCache, _config.NumberOfParalellSegmentTasks, segmentTaskQueue);

            var splitChangeFetcher = new ApiSplitChangeFetcher(_splitSdkApiClient);
            _splitParser = new InMemorySplitParser((SelfRefreshingSegmentFetcher)_selfRefreshingSegmentFetcher, _segmentCache);
            _splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>(_config.ConcurrencyLevel, InitialCapacity));
            _splitFetcher = new SelfRefreshingSplitFetcher(splitChangeFetcher, _splitParser, _gates, splitsRefreshRate, _splitCache);

            _trafficTypeValidator = new TrafficTypeValidator(_splitCache);
        }

        private void BuildTreatmentLog(ConfigurationOptions config)
        {
            var impressionsCache = new InMemorySimpleCache<KeyImpression>(new BlockingQueue<KeyImpression>(_config.TreatmentLogSize));
            _impressionsLog = new ImpressionsLog(_treatmentSdkApiClient, _config.TreatmentLogRefreshRate, impressionsCache);

            _customerImpressionListener = config.ImpressionListener;
        }

        private void BuildImpressionManager()
        {
            var impressionsHasher = new ImpressionHasher();
            var impressionsObserver = new ImpressionsObserver(impressionsHasher);
            _impressionsCounter = new ImpressionsCounter();
            _impressionsManager = new ImpressionsManager(_impressionsLog, _customerImpressionListener, _impressionsCounter, true, _config.ImpressionsMode, impressionsObserver);
        }

        private void BuildEventLog(ConfigurationOptions config)
        {
            var eventsCache = new InMemorySimpleCache<WrappedEvent>(new BlockingQueue<WrappedEvent>(_config.EventLogSize));
            _eventsLog = new EventsLog(_eventSdkApiClient, _config.EventsFirstPushWindow, _config.EventLogRefreshRate, eventsCache);
        }
        
        private void BuildMetricsLog()
        {
            _metricsCache = new InMemoryMetricsCache(new ConcurrentDictionary<string, Counter>(), new ConcurrentDictionary<string, ILatencyTracker>(), new ConcurrentDictionary<string, long>());
            _metricsLog = new AsyncMetricsLog(_metricsSdkApiClient, _metricsCache, _config.MaxCountCalls, _config.MaxTimeBetweenCalls);
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
                splitSDKVersion = _config.SdkVersion,
                splitSDKSpecVersion = _config.SdkSpecVersion,
                splitSDKMachineName = _config.SdkMachineName,
                splitSDKMachineIP = _config.SdkMachineIP,
                SplitSDKImpressionsMode = _config.ImpressionsMode.Equals(ImpressionsMode.Optimized)
            };

            _metricsSdkApiClient = new MetricsSdkApiClient(header, _config.EventsBaseUrl, _config.HttpConnectionTimeout, _config.HttpReadTimeout);
            BuildMetricsLog();
            _splitSdkApiClient = new SplitSdkApiClient(header, _config.BaseUrl, _config.HttpConnectionTimeout, _config.HttpReadTimeout, _metricsLog);
            _segmentSdkApiClient = new SegmentSdkApiClient(header, _config.BaseUrl, _config.HttpConnectionTimeout, _config.HttpReadTimeout, _metricsLog);
            _treatmentSdkApiClient = new TreatmentSdkApiClient(header, _config.EventsBaseUrl, _config.HttpConnectionTimeout, _config.HttpReadTimeout);
            _eventSdkApiClient = new EventSdkApiClient(header, _config.EventsBaseUrl, _config.HttpConnectionTimeout, _config.HttpReadTimeout);
        }

        private void BuildManager()
        {
            _manager = new SplitManager(_splitCache, _blockUntilReadyService);
        }

        private void BuildBlockUntilReadyService()
        {
            _blockUntilReadyService = new SelfRefreshingBlockUntilReadyService(_gates, _log);
        }

        private void BuildSyncManager()
        {
            try
            {
                var impressionsCountSender = new ImpressionsCountSender(_treatmentSdkApiClient, _impressionsCounter);
                var synchronizer = new Synchronizer(_splitFetcher, _selfRefreshingSegmentFetcher, _impressionsLog, _eventsLog, _metricsLog, impressionsCountSender, _wrapperAdapter);
                var splitsWorker = new SplitsWorker(_splitCache, synchronizer);
                var segmentsWorker = new SegmentsWorker(_segmentCache, synchronizer);
                var notificationProcessor = new NotificationProcessor(splitsWorker, segmentsWorker);
                var notificationParser = new NotificationParser();
                var eventSourceClient = new EventSourceClient(notificationParser: notificationParser);
                var notificationManagerKeeper = new NotificationManagerKeeper();
                var sseHandler = new SSEHandler(_config.StreamingServiceURL, splitsWorker, segmentsWorker, notificationProcessor, notificationManagerKeeper, eventSourceClient: eventSourceClient);
                var authApiClient = new AuthApiClient(_config.AuthServiceURL, ApiKey, _config.HttpReadTimeout);
                var pushManager = new PushManager(_config.AuthRetryBackoffBase, sseHandler, authApiClient, _wrapperAdapter);

                _syncManager = new SyncManager(_config.StreamingEnabled, synchronizer, pushManager, sseHandler, notificationManagerKeeper);
            }
            catch (Exception ex)
            {
                _log.Error($"BuildSyncManager: {ex.Message}");
            }
        }

        private void Start()
        {
            _syncManager.Start();
        }

        private void Stop()
        {
            _syncManager.Shutdown();
        }

        private static ISplitLogger GetLogger(ISplitLogger splitLogger = null)
        {
            return splitLogger ?? WrapperAdapter.GetLogger(typeof(SelfRefreshingClient));
        }
        #endregion
    }
}
