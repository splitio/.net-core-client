using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionsManager : IImpressionsManager
    {
        private readonly IImpressionsObserver _impressionsObserver;
        private readonly IImpressionsLog _impressionsLog;
        private readonly IImpressionListener _customerImpressionListener;
        private readonly IImpressionsCounter _impressionsCounter;
        private readonly bool _optimized;
        private readonly bool _addPreviousTime;

        public ImpressionsManager(IImpressionsLog impressionsLog,
            IImpressionListener customerImpressionListener,
            IImpressionsCounter impressionsCounter,
            bool addPreviousTime,
            ImpressionModes impressionMode,
            IImpressionsObserver impressionsObserver = null)
        {            
            _impressionsLog = impressionsLog;
            _customerImpressionListener = customerImpressionListener;
            _impressionsCounter = impressionsCounter;
            _addPreviousTime = addPreviousTime;
            _optimized = impressionMode == ImpressionModes.Optimized && addPreviousTime;
            _impressionsObserver = impressionsObserver;
        }

        public KeyImpression BuildImpression(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey)
        {
            var impression = new KeyImpression(matchingKey, feature, treatment, time, changeNumber, label, bucketingKey);

            if (_addPreviousTime && _impressionsObserver != null)
            {
                impression.previousTime = _impressionsObserver.TestAndSet(impression);
            }

            if (_optimized)
            {
                _impressionsCounter.Inc(feature, time);
            }

            return impression;
        }

        public void BuildAndTrack(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey)
        {
            Track(new List<KeyImpression>()
            {
                BuildImpression(matchingKey, feature, treatment, time, changeNumber, label, bucketingKey)
            });
        }

        public void Track(List<KeyImpression> impressions)
        {
            if (impressions.Any())
            {
                if (_impressionsLog != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        if (_optimized)
                        {
                            var optimizedImpressions = impressions.Where(i => ShouldQueueImpression(i)).ToList();
                            _impressionsLog.Log(optimizedImpressions);
                        }
                        else
                        {
                            _impressionsLog.Log(impressions);
                        }                        
                    });
                }

                if (_customerImpressionListener != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        foreach (var imp in impressions)
                        {
                            _customerImpressionListener.Log(imp);
                        }
                    });
                }
            }
        }        

        public bool ShouldQueueImpression(KeyImpression impression)
        {
            return impression.previousTime == null || (ImpressionsHelper.TruncateTimeFrame(impression.previousTime.Value) != ImpressionsHelper.TruncateTimeFrame(impression.time));
        }
    }
}
