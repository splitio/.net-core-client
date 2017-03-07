using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using System.Threading.Tasks;

namespace Splitio.Services.Impressions.Classes
{
    public class RedisTreatmentLog : ITreatmentLog
    {
        private IImpressionsCache impressionsCache;

        public RedisTreatmentLog(IImpressionsCache impressionsCache)
        {
            this.impressionsCache = impressionsCache;
        }


        public void Log(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey = null)
        {
            KeyImpression impression = new KeyImpression() { feature = feature, keyName = matchingKey, treatment = treatment, time = time, changeNumber = changeNumber, label = label, bucketingKey = bucketingKey };
            var enqueueTask = new Task(() => impressionsCache.AddImpression(impression));
            enqueueTask.Start();
        }
    }
}
