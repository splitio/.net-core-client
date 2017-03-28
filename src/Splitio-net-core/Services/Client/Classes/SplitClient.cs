using NLog;
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
        protected static readonly Logger Log = LogManager.GetLogger(typeof(SplitClient).ToString());
        protected const string Control = "control";
        protected const string SdkGetTreatment = "sdk.getTreatment";
        protected const string LabelKilled = "killed";
        protected const string LabelNoConditionMatched = "no rule matched";
        protected const string LabelSplitNotFound = "rules not found";
        protected const string LabelException = "exception";

        protected static bool LabelsEnabled;

        protected Splitter splitter;
        protected ITreatmentLog treatmentLog;
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

            if (treatmentLog != null)
            {
                treatmentLog.Log(key.matchingKey, feature, treatment, start, changeNumber, LabelsEnabled ? label : null, key.bucketingKeyHadValue ? key.bucketingKey : null);
            }
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

                Log.Error(e, string.Format("Exception caught getting treatment for feature: {0}", feature));
                return Control;
            }
        }

        protected string GetTreatment(Key key, ParsedSplit split, Dictionary<string, object> attributes, long start, Stopwatch clock)
        {
            if (!split.killed)
            {
                foreach (ConditionWithLogic condition in split.conditions)
                {
                    var combiningMatcher = condition.matcher;
                    if (combiningMatcher.Match(key.matchingKey, attributes))
                    {
                        var treatment = splitter.GetTreatment(key.bucketingKey, split.seed, condition.partitions);

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
