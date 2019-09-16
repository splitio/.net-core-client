using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.Evaluator;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.Parsing.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
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

        protected bool LabelsEnabled;
        protected bool Destroyed;
        protected string ApiKey;

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
        protected ISplitParser _splitParser;
        protected IEvaluator _evaluator;

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
        public SplitResult GetTreatmentWithConfig(string key, string feature, Dictionary<string, object> attributes = null)
        {
            return GetTreatmentWithConfig(new Key(key, null), feature, attributes);
        }

        public SplitResult GetTreatmentWithConfig(Key key, string feature, Dictionary<string, object> attributes = null)
        {
            var result = GetTreatmentResult(key, feature, SdkGetTreatmentWithConfig, nameof(GetTreatmentWithConfig), attributes);

            return new SplitResult
            {
                Treatment = result.Treatment,
                Config = result.Config
            };
        }

        public virtual string GetTreatment(string key, string feature, Dictionary<string, object> attributes = null)
        {
            return GetTreatment(new Key(key, null), feature, attributes);
        }

        public virtual string GetTreatment(Key key, string feature, Dictionary<string, object> attributes = null)
        {
            var result = GetTreatmentResult(key, feature, SdkGetTreatment, nameof(GetTreatment), attributes);

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

        public void BlockUntilReady(int blockMilisecondsUntilReady)
        {
            _blockUntilReadyService.BlockUntilReady(blockMilisecondsUntilReady);
        }
        #endregion

        #region Protected Methods
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

        protected void BuildEvaluator(ILog log = null)
        {
            _evaluator = new Evaluator.Evaluator(splitCache, _splitParser, log: log);
        }
        #endregion

        #region Private Methods
        private TreatmentResult GetTreatmentResult(Key key, string feature, string operation, string method, Dictionary<string, object> attributes = null)
        {
            if (!IsClientReady(method)) return new TreatmentResult(string.Empty, Control, null);

            if (!_keyValidator.IsValid(key, method)) return new TreatmentResult(string.Empty, Control, null);

            var splitNameResult = _splitNameValidator.SplitNameIsValid(feature, method);

            if (!splitNameResult.Success) return new TreatmentResult(string.Empty, Control, null);

            feature = splitNameResult.Value;

            var result = _evaluator.EvaluateFeature(key, feature, attributes);

            if (metricsLog != null)
            {
                metricsLog.Time(operation, result.ElapsedMilliseconds);
            }

            if (!Labels.LabelSplitNotFound.Equals(result.Label))
            {
                ImpressionLog(new List<KeyImpression>
                {
                    BuildImpression(key.matchingKey, feature, result.Treatment, CurrentTimeHelper.CurrentTimeMillis(), result.ChangeNumber, LabelsEnabled ? result.Label : null, key.bucketingKeyHadValue ? key.bucketingKey : null)
                });
            }

            return result;
        }

        private Dictionary<string, TreatmentResult> GetTreatmentsResult(Key key, List<string> features, string operation, string method, Dictionary<string, object> attributes = null)
        {
            var treatmentsForFeatures = new Dictionary<string, TreatmentResult>();

            if (!IsClientReady(method))
            {
                foreach (var feature in features)
                {
                    treatmentsForFeatures.Add(feature, new TreatmentResult(string.Empty, Control, null));
                }

                return treatmentsForFeatures;
            }
            
            var ImpressionsQueue = new List<KeyImpression>();

            if (_keyValidator.IsValid(key, method))
            {
                features = _splitNameValidator.SplitNamesAreValid(features, method);
                
                var results = _evaluator.EvaluateFeatures(key, features, attributes);

                foreach (var treatmentResult in results.TreatmentResults)
                {
                    treatmentsForFeatures.Add(treatmentResult.Key, treatmentResult.Value);

                    if (!Labels.LabelSplitNotFound.Equals(treatmentResult.Value.Label))
                    {
                        ImpressionsQueue.Add(BuildImpression(key.matchingKey, treatmentResult.Key, treatmentResult.Value.Treatment, CurrentTimeHelper.CurrentTimeMillis(), treatmentResult.Value.ChangeNumber, LabelsEnabled ? treatmentResult.Value.Label : null, key.bucketingKeyHadValue ? key.bucketingKey : null));
                    }
                }

                if (metricsLog != null)
                {
                    metricsLog.Time(operation, results.ElapsedMilliseconds);
                }

                ImpressionLog(ImpressionsQueue);
            }
            else
            {
                foreach (var feature in features)
                {
                    treatmentsForFeatures.Add(feature, new TreatmentResult(string.Empty, Control, null));
                }                    
            }

            return treatmentsForFeatures;
        }
        
        private KeyImpression BuildImpression(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey)
        {
            return new KeyImpression { feature = feature, keyName = matchingKey, treatment = treatment, time = time, changeNumber = changeNumber, label = label, bucketingKey = bucketingKey };
        }
        #endregion
    }
}