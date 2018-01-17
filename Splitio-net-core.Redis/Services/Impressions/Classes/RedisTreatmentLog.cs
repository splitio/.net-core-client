using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Impressions.Interfaces;

namespace Splitio.Redis.Services.Impressions.Classes
{
    public class RedisTreatmentLog : IImpressionListener
    {
        private ISimpleCache<KeyImpression> impressionsCache;

        public RedisTreatmentLog(ISimpleCache<KeyImpression> impressionsCache)
        {
            this.impressionsCache = impressionsCache;
        }

        public void Log(KeyImpression impression)
        {
            impressionsCache.AddItem(impression);
        }
    }
}