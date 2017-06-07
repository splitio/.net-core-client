using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Metrics.Interfaces;
using System;
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
        protected const string LabelNoConditionMatched = "no rule matched";
        protected const string LabelSplitNotFound = "rules not found";
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

        public ISplitManager GetSplitManager()
        {
            return manager;
        }

        public string GetTreatment(string key, string feature, Dictionary<string, object> attributes = null)
        {
            Key keys = new Key(key, null);
            return GetTreatmentForFeature(keys, feature, attributes);
        }

        public string GetTreatment(Key key, string feature, Dictionary<string, object> attributes = null)
        {
            return GetTreatmentForFeature(key, feature, attributes);
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

    protected virtual string GetTreatmentForFeature(Key key, string feature, Dictionary<string, object> attributes)
        {
            long start = CurrentTimeHelper.CurrentTimeMillis();
            var clock = new Stopwatch();
            clock.Start();

            try
            {
                var split = (ParsedSplit)splitCache.GetSplit(feature);

                if (split == null)
                {
                    //if split definition was not found, impression label = "rules not found"
                    RecordStats(key, feature, null, LabelSplitNotFound, start, Control, SdkGetTreatment, clock);

                    Log.Warn(string.Format("Unknown or invalid feature: {0}", feature));
                    
                    return Control;
                }
                
                var treatment = GetTreatment(key, split, attributes, start, clock);
                
                return treatment;
            }
            catch (Exception e)
            {
                //if there was an exception, impression label = "exception"
                RecordStats(key, feature, null, LabelException, start, Control, SdkGetTreatment, clock);

                Log.Error(string.Format("Exception caught getting treatment for feature: {0}", feature), e);
                return Control;
            }
        }

        protected string GetTreatment(Key key, ParsedSplit split, Dictionary<string, object> attributes, long start, Stopwatch clock)
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
                                // If not in traffic allocation, abort and return
                                // default treatment
                                RecordStats(key, split.name, split.changeNumber, LabelTrafficAllocationFailed, start, split.defaultTreatment, SdkGetTreatment, clock);

                                return split.defaultTreatment;
                            }
                        }
                        inRollout = true;
                    }
                    var combiningMatcher = condition.matcher;
                    if (combiningMatcher.Match(key.matchingKey, attributes))
                    {
                        var treatment = splitter.GetTreatment(key.bucketingKey, split.seed, condition.partitions, split.algo);

                        //If condition matched, impression label = condition.label 
                        RecordStats(key, split.name, split.changeNumber, condition.label, start, treatment, SdkGetTreatment, clock);

                        return treatment;
                    }
                }
                //If no condition matched, impression label = "no rule matched"
                RecordStats(key, split.name, split.changeNumber, LabelNoConditionMatched, start, split.defaultTreatment, SdkGetTreatment, clock);

                return split.defaultTreatment;
            }
            else
            {
                //If split was killed, impression label = "killed"
                RecordStats(key, split.name, split.changeNumber, LabelKilled, start, split.defaultTreatment, SdkGetTreatment, clock);

                return split.defaultTreatment;
            }
        }

        public Dictionary<string, string> GetTreatments(string key, List<string> features, Dictionary<string, object> attributes = null)
        {
            Key keys = new Key(key, null);
            Dictionary<string, string> treatmentsForFeatures;
            treatmentsForFeatures = features.ToDictionary(x => x, x => GetTreatment(keys, x, attributes));
            return treatmentsForFeatures;
        }

        public Dictionary<string, string> GetTreatments(Key key, List<string> features, Dictionary<string, object> attributes = null)
        {
            Dictionary<string, string> treatmentsForFeatures;
            treatmentsForFeatures = features.ToDictionary(x => x, x => GetTreatment(key, x, attributes));
            return treatmentsForFeatures;
        }
    }
}
