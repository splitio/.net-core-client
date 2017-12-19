using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Metrics.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Splitio.Services.Client.Classes
{
    public abstract class SplitClient: ISplitClient
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(SplitClient));
        protected const string Control = "control";
        protected const string SdkGetTreatment = "sdk.getTreatment";
        protected const string LabelKilled = "killed";
        protected const string LabelDefaultRule = "default rule";
        protected const string LabelSplitNotFound = "definition not found";
        protected const string LabelException = "exception";
        protected const string LabelTrafficAllocationFailed = "not in split";

        protected static bool LabelsEnabled;

        protected Splitter splitter;
        protected IImpressionListener impressionListener;
        protected IMetricsLog metricsLog;
        protected ISplitManager manager;
        protected IMetricsCache metricsCache;
        protected IImpressionsCache impressionsCache;
        protected ISplitCache splitCache;
        protected ISegmentCache segmentCache;

        private ConcurrentDictionary<string, string> treatmentCache = new ConcurrentDictionary<string, string>();

        public ISplitManager GetSplitManager()
        {
            return manager;
        }

        public string GetTreatment(string key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false)
        {
            Key keys = new Key(key, null);
            return GetTreatment(keys, feature, attributes, logMetricsAndImpressions, multiple);
        }

        public string GetTreatment(Key key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false)
        {
            string featureHash = string.Concat(key.matchingKey, "#", feature, "#", attributes != null ? attributes.GetHashCode() : 0);

            if (multiple && treatmentCache.ContainsKey(featureHash))
            {
                return treatmentCache[featureHash];
            }

            var result = GetTreatmentForFeature(key, feature, attributes, logMetricsAndImpressions);

            if (multiple)
            {
                treatmentCache.TryAdd(featureHash, result);
            }

            return result;
        }

        protected void RecordStats(Key key, string feature, long? changeNumber, string label, long start, string treatment, string operation, Stopwatch clock)
        {
            if (metricsLog != null)
            {
                metricsLog.Time(SdkGetTreatment, clock.ElapsedMilliseconds);
            }

            if (impressionListener != null)
            {
                KeyImpression impression = BuildImpression(key.matchingKey, feature, treatment, start, changeNumber, LabelsEnabled ? label : null, key.bucketingKeyHadValue ? key.bucketingKey : null);
                impressionListener.Log(impression);
            }
        }

        private KeyImpression BuildImpression(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey)
        {
            return new KeyImpression() { feature = feature, keyName = matchingKey, treatment = treatment, time = time, changeNumber = changeNumber, label = label, bucketingKey = bucketingKey };
        }

        protected virtual string GetTreatmentForFeature(Key key, string feature, Dictionary<string, object> attributes, bool logMetricsAndImpressions = true)
        {
            long start = CurrentTimeHelper.CurrentTimeMillis();
            var clock = new Stopwatch();
            clock.Start();

            try
            {
                var split = (ParsedSplit)splitCache.GetSplit(feature);

                if (split == null)
                {
                    if (logMetricsAndImpressions)
                    {
                        //if split definition was not found, impression label = "definition not found"
                        RecordStats(key, feature, null, LabelSplitNotFound, start, Control, SdkGetTreatment, clock);
                    }

                    Log.Warn(string.Format("Unknown or invalid feature: {0}", feature));
                    
                    return Control;
                }

                var treatment = GetTreatment(key, split, attributes, start, clock, this, logMetricsAndImpressions);

                return treatment;
            }
            catch (Exception e)
            {
                if (logMetricsAndImpressions)
                {
                    //if there was an exception, impression label = "exception"
                    RecordStats(key, feature, null, LabelException, start, Control, SdkGetTreatment, clock);
                }

                Log.Error(string.Format("Exception caught getting treatment for feature: {0}", feature), e);
                return Control;
            }
        }

        protected string GetTreatment(Key key, ParsedSplit split, Dictionary<string, object> attributes, long start, Stopwatch clock, ISplitClient splitClient, bool logMetricsAndImpressions)
        {
            if (!split.killed)
            {
                bool inRollout = false;
                // use the first matching condition
                foreach (ConditionWithLogic condition in split.conditions)
                {
                    if (!inRollout && condition.conditionType == ConditionType.ROLLOUT)
                    {
                        if (split.trafficAllocation < 100)
                        {
                            // bucket ranges from 1-100.
                            int bucket = split.algo == AlgorithmEnum.LegacyHash ? splitter.LegacyBucket(key.bucketingKey, split.trafficAllocationSeed) : splitter.Bucket(key.bucketingKey, split.trafficAllocationSeed);
                            if (bucket >= split.trafficAllocation)
                            {
                                if (logMetricsAndImpressions)
                                {
                                    // If not in traffic allocation, abort and return
                                    // default treatment
                                    RecordStats(key, split.name, split.changeNumber, LabelTrafficAllocationFailed, start, split.defaultTreatment, SdkGetTreatment, clock);
                                }

                                return split.defaultTreatment;
                            }
                        }
                        inRollout = true;
                    }
                    var combiningMatcher = condition.matcher;
                    if (combiningMatcher.Match(key, attributes, splitClient))
                    {
                        var treatment = splitter.GetTreatment(key.bucketingKey, split.seed, condition.partitions, split.algo);

                        if (logMetricsAndImpressions)
                        {
                            //If condition matched, impression label = condition.label 
                            RecordStats(key, split.name, split.changeNumber, condition.label, start, treatment, SdkGetTreatment, clock);
                        }

                        return treatment;
                    }
                }

                if (logMetricsAndImpressions)
                {
                    //If no condition matched, impression label = "default rule"
                    RecordStats(key, split.name, split.changeNumber, LabelDefaultRule, start, split.defaultTreatment, SdkGetTreatment, clock);
                }
                return split.defaultTreatment;
            }
            else
            {
                if (logMetricsAndImpressions)
                {
                    //If split was killed, impression label = "killed"
                    RecordStats(key, split.name, split.changeNumber, LabelKilled, start, split.defaultTreatment, SdkGetTreatment, clock);
                }

                return split.defaultTreatment;
            }
        }

        public Dictionary<string, string> GetTreatments(string key, List<string> features, Dictionary<string, object> attributes = null)
        {
            Key keys = new Key(key, null);
            return GetTreatments(keys, features, attributes);
        }

        public Dictionary<string, string> GetTreatments(Key key, List<string> features, Dictionary<string, object> attributes = null)
        {
            Dictionary<string, string> treatmentsForFeatures = new Dictionary<string, string>();

            foreach (string feature in features)
            {
                treatmentsForFeatures.Add(feature, GetTreatment(key, feature, attributes, true, true));
            }

            ClearItemsAddedToTreatmentCache(key.matchingKey);
            return treatmentsForFeatures;
        }

        private void ClearItemsAddedToTreatmentCache(string key)
        {
            var temporaryTreatmentCache = new ConcurrentDictionary<string, string>(treatmentCache);
            foreach (var item in temporaryTreatmentCache.Keys.Where(x => x.StartsWith(key)))
            {
                string result;
                temporaryTreatmentCache.TryRemove(item, out result);
            }
            treatmentCache = temporaryTreatmentCache;
        }

        public abstract void Destroy();
    }
}
