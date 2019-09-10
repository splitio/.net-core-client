using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Splitio.Services.Client.Classes
{
    public abstract class SplitClient : ISplitClient
    {
        protected readonly ILog _log;
        protected readonly IKeyValidator _keyValidator;
        protected readonly ISplitNameValidator _splitNameValidator;
        protected readonly IEventTypeValidator _eventTypeValidator;
        protected readonly IEventPropertiesValidator _eventPropertiesValidator;
        protected readonly IWrapperAdapter _wrapperAdapter;
        protected const string Control = "control";
        protected const string SdkGetTreatment = "sdk.getTreatment";
        protected const string SdkGetTreatments = "sdk.getTreatments";
        protected const string SdkGetTreatmentWithConfig = "sdk.getTreatmentWithConfig";
        protected const string SdkGetTreatmentsWithConfig = "sdk.getTreatmentsWithConfig";
        protected const string LabelKilled = "killed";
        protected const string LabelDefaultRule = "default rule";
        protected const string LabelSplitNotFound = "definition not found";
        protected const string LabelException = "exception";
        protected const string LabelTrafficAllocationFailed = "not in split";
        protected const string LabelClientNotReady = "not ready";

        protected bool LabelsEnabled;
        protected bool Destroyed;
        protected string ApiKey;
        
        protected Splitter splitter;
        protected IListener<KeyImpression> impressionListener;
        protected IListener<WrappedEvent> eventListener;
        protected IMetricsLog metricsLog;
        protected ISplitManager manager;
        protected IMetricsCache metricsCache;
        protected ISimpleCache<KeyImpression> impressionsCache;
        protected ISimpleCache<WrappedEvent> eventsCache;
        protected ISplitCache splitCache;
        protected ITrafficTypeValidator _trafficTypeValidator;
        protected ISegmentCache segmentCache;
        protected IBlockUntilReadyService _blockUntilReadyService;
        protected IFactoryInstantiationsService _factoryInstantiationsService;

        private ConcurrentDictionary<string, string> treatmentCache = new ConcurrentDictionary<string, string>();

        public SplitClient(ILog log)
        {
            _log = log;
            _keyValidator = new KeyValidator(_log);
            _splitNameValidator = new SplitNameValidator(_log);
            _eventTypeValidator = new EventTypeValidator(_log);
            _eventPropertiesValidator = new EventPropertiesValidator(_log);
            _factoryInstantiationsService = FactoryInstantiationsService.Instance(log);
            _wrapperAdapter = new WrapperAdapter();
        }

        public ISplitManager GetSplitManager()
        {
            return manager;
        }

        #region Public Methods
        public SplitResult GetTreatmentWithConfig(string key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false)
        {
            return GetTreatmentWithConfig(new Key(key, null), feature, attributes, logMetricsAndImpressions, multiple);
        }

        public SplitResult GetTreatmentWithConfig(Key key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false)
        {
            var result = GetTreatmentResult(key, feature, SdkGetTreatmentWithConfig, nameof(GetTreatmentWithConfig), attributes, logMetricsAndImpressions, multiple);

            return new SplitResult
            {
                Treatment = result.Treatment,
                Config = result.Config
            };
        }

        public string GetTreatment(string key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false)
        {
            return GetTreatment(new Key(key, null), feature, attributes, logMetricsAndImpressions, multiple);
        }

        public string GetTreatment(Key key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false)
        {
            var result = GetTreatmentResult(key, feature, SdkGetTreatment, nameof(GetTreatment), attributes, logMetricsAndImpressions, multiple);

            return result.Treatment;
        }

        public Dictionary<string, SplitResult> GetTreatmentsWithConfig(string key, List<string> features, Dictionary<string, object> attributes = null)
        {
            return GetTreatmentsWithConfig(new Key(key, null), features, attributes);
        }

        public Dictionary<string, SplitResult> GetTreatmentsWithConfig(Key key, List<string> features, Dictionary<string, object> attributes = null)
        {
            var results = GetTreatmentsResult(key, features, SdkGetTreatmentsWithConfig, nameof(GetTreatmentsWithConfig), attributes);

            return results
                .ToDictionary(r => r.Key, r => new SplitResult
                {
                    Treatment = r.Value.Treatment,
                    Config = r.Value.Config
                });
        }

        public Dictionary<string, string> GetTreatments(string key, List<string> features, Dictionary<string, object> attributes = null)
        {
            return GetTreatments(new Key(key, null), features, attributes);
        }

        public Dictionary<string, string> GetTreatments(Key key, List<string> features, Dictionary<string, object> attributes = null)
        {
            var results = GetTreatmentsResult(key, features, SdkGetTreatments, nameof(GetTreatments), attributes);

            return results
                .ToDictionary(r => r.Key, r => r.Value.Treatment);
        }

        public virtual bool Track(string key, string trafficType, string eventType, double? value = null, Dictionary<string, object> properties = null)
        {
            if (Destroyed) return false;

            var keyResult = _keyValidator.IsValid(new Key(key, null), nameof(Track));            
            var eventTypeResult = _eventTypeValidator.IsValid(eventType, nameof(eventType));
            var eventPropertiesResult = _eventPropertiesValidator.IsValid(properties);

            var trafficTypeResult = _blockUntilReadyService.IsSdkReady()
                ? _trafficTypeValidator.IsValid(trafficType, nameof(trafficType))
                : new ValidatorResult { Success = true };

            if (!keyResult || !trafficTypeResult.Success || !eventTypeResult || !eventPropertiesResult.Success)
                return false;

            try
            {
                var eventToLog = new Event
                {
                    key = key,
                    trafficTypeName = trafficTypeResult.Value,
                    eventTypeId = eventType,
                    value = value,
                    timestamp = CurrentTimeHelper.CurrentTimeMillis(),
                    properties = (Dictionary<string, object>)eventPropertiesResult.Value
                };

                eventListener.Log(new WrappedEvent
                {
                    Event = eventToLog,
                    Size = eventPropertiesResult.EventSize
                });

                return true;
            }
            catch (Exception e)
            {
                _log.Error("Exception caught trying to track an event", e);
                return false;
            }
        }

        public bool IsDestroyed()
        {
            return Destroyed;
        }

        public virtual void Destroy()
        {
            if (!Destroyed)
            {
                _factoryInstantiationsService.Decrease(ApiKey);
                Destroyed = true;
            }
        }

        public abstract void BlockUntilReady(int blockMilisecondsUntilReady);
        #endregion

        #region Protected Methods
        protected virtual TreatmentResult GetTreatmentForFeature(Key key, string feature, Dictionary<string, object> attributes)
        {
            try
            {
                var split = (ParsedSplit)splitCache.GetSplit(feature);

                if (split == null)
                {
                    _log.Warn($"GetTreatment: you passed {feature} that does not exist in this environment, please double check what Splits exist in the web console.");

                    return new TreatmentResult(LabelSplitNotFound, Control, null);
                }

                var treatmentResult = GetTreatment(key, split, attributes, this);

                if (split.configurations != null && split.configurations.ContainsKey(treatmentResult.Treatment))
                {
                    treatmentResult.Config = split.configurations[treatmentResult.Treatment];
                }
                
                return treatmentResult;
            }
            catch (Exception e)
            {
                _log.Error($"Exception caught getting treatment for feature: {feature}", e);

                return new TreatmentResult(LabelException, Control, null);
            }
        }

        protected TreatmentResult GetTreatment(Key key, ParsedSplit split, Dictionary<string, object> attributes, ISplitClient splitClient)
        {
            if (!split.killed)
            {
                bool inRollout = false;
                // use the first matching condition
                foreach (var condition in split.conditions)
                {
                    if (!inRollout && condition.conditionType == ConditionType.ROLLOUT)
                    {
                        if (split.trafficAllocation < 100)
                        {
                            // bucket ranges from 1-100.
                            var bucket = split.algo == AlgorithmEnum.LegacyHash 
                                ? splitter.LegacyBucket(key.bucketingKey, split.trafficAllocationSeed) 
                                : splitter.Bucket(key.bucketingKey, split.trafficAllocationSeed);

                            if (bucket > split.trafficAllocation)
                            {
                                return new TreatmentResult(LabelTrafficAllocationFailed, split.defaultTreatment, split.changeNumber);
                            }
                        }

                        inRollout = true;
                    }

                    var combiningMatcher = condition.matcher;

                    if (combiningMatcher.Match(key, attributes, splitClient))
                    {
                        var treatment = splitter.GetTreatment(key.bucketingKey, split.seed, condition.partitions, split.algo);

                        return new TreatmentResult(condition.label, treatment, split.changeNumber);
                    }
                }

                return new TreatmentResult(LabelDefaultRule, split.defaultTreatment, split.changeNumber);
            }
            else
            {
                return new TreatmentResult(LabelKilled, split.defaultTreatment, split.changeNumber);
            }
        }

        protected virtual void ImpressionLog(List<KeyImpression> impressionsQueue)
        {
            if (impressionListener != null)
            {
                foreach (var imp in impressionsQueue)
                {
                    impressionListener.Log(imp);
                }
            }
        }

        protected bool IsClientReady(string methodName)
        {
            if (!_blockUntilReadyService.IsSdkReady())
            {
                _log.Error($"{methodName}: the SDK is not ready, the operation cannot be executed.");
                return false;
            }

            if (Destroyed)
            {
                _log.Error("Client has already been destroyed - No calls possible");
            }

            return true;
        }
        #endregion

        #region Private Methods
        private Dictionary<string, TreatmentResult> GetTreatmentsResult(Key key, List<string> features, string operation, string method, Dictionary<string, object> attributes = null)
        {
            var treatmentsForFeatures = new Dictionary<string, TreatmentResult>();

            if (!IsClientReady(method))
            {
                foreach (var feature in features)
                {
                    treatmentsForFeatures.Add(feature, new TreatmentResult(LabelClientNotReady, Control, null));
                }

                return treatmentsForFeatures;
            }
            
            var ImpressionsQueue = new List<KeyImpression>();

            if (_keyValidator.IsValid(key, method))
            {
                features = _splitNameValidator.SplitNamesAreValid(features, method);

                var start = CurrentTimeHelper.CurrentTimeMillis();
                var clock = new Stopwatch();
                clock.Start();

                foreach (var feature in features)
                {
                    var treatmentResult = DoGetTreatment(key, feature, attributes, true);

                    treatmentsForFeatures.Add(feature, treatmentResult);

                    if (!LabelSplitNotFound.Equals(treatmentResult.Label))
                    {
                        ImpressionsQueue.Add(BuildImpression(key.matchingKey, feature, treatmentResult.Treatment, start, treatmentResult.ChangeNumber, LabelsEnabled ? treatmentResult.Label : null, key.bucketingKeyHadValue ? key.bucketingKey : null));
                    }
                }

                if (metricsLog != null)
                {
                    metricsLog.Time(operation, clock.ElapsedMilliseconds);
                }

                ImpressionLog(ImpressionsQueue);
            }
            else
            {
                foreach (var feature in features)
                {
                    treatmentsForFeatures.Add(feature, new TreatmentResult(LabelSplitNotFound, Control, null));
                }                    
            }

            ClearItemsAddedToTreatmentCache(key?.matchingKey);

            return treatmentsForFeatures;
        }

        private TreatmentResult GetTreatmentResult(Key key, string feature, string operation, string method, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false)
        {
            if (!IsClientReady(method)) return new TreatmentResult(LabelClientNotReady, Control, null);

            if (!_keyValidator.IsValid(key, method)) return new TreatmentResult(LabelException, Control, null);

            var splitNameResult = _splitNameValidator.SplitNameIsValid(feature, method);

            if (!splitNameResult.Success) return new TreatmentResult(LabelException, Control, null);

            feature = splitNameResult.Value;

            var start = CurrentTimeHelper.CurrentTimeMillis();
            var clock = new Stopwatch();
            clock.Start();

            var result = DoGetTreatment(key, feature, attributes, multiple);

            if (logMetricsAndImpressions)
            {
                if (metricsLog != null)
                {
                    metricsLog.Time(operation, clock.ElapsedMilliseconds);
                }

                if (!LabelSplitNotFound.Equals(result.Label))
                {
                    ImpressionLog(new List<KeyImpression>
                    {
                        BuildImpression(key.matchingKey, feature, result.Treatment, start, result.ChangeNumber, LabelsEnabled ? result.Label : null, key.bucketingKeyHadValue ? key.bucketingKey : null)
                    });
                }
            }

            return result;
        }

        private TreatmentResult DoGetTreatment(Key key, string feature, Dictionary<string, object> attributes = null, bool multiple = false)
        {
            if (feature == null)
            {
                _log.Error($"{nameof(GetTreatment)}: split_name cannot be null");

                return new TreatmentResult(string.Empty, Control, null);
            }

            var featureHash = string.Concat(key.matchingKey, "#", feature, "#", attributes != null ? attributes.GetHashCode() : 0);

            if (multiple && treatmentCache.ContainsKey(featureHash))
            {
                return new TreatmentResult(string.Empty, treatmentCache[featureHash], null);
            }

            var result = GetTreatmentForFeature(key, feature, attributes);

            if (multiple)
            {
                treatmentCache.TryAdd(featureHash, result.Treatment);
            }

            return result;
        }

        private KeyImpression BuildImpression(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey)
        {
            return new KeyImpression { feature = feature, keyName = matchingKey, treatment = treatment, time = time, changeNumber = changeNumber, label = label, bucketingKey = bucketingKey };
        }

        private void ClearItemsAddedToTreatmentCache(string key)
        {
            var temporaryTreatmentCache = new ConcurrentDictionary<string, string>(treatmentCache);
            foreach (var item in temporaryTreatmentCache.Keys.Where(x => x.StartsWith(key)))
            {
                temporaryTreatmentCache.TryRemove(item, out string result);
            }

            treatmentCache = temporaryTreatmentCache;
        }
        #endregion
    }
}