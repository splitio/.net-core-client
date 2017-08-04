using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Impressions.Interfaces;

namespace Splitio.Redis.Services.Impressions.Classes
{
    public class RedisTreatmentLog : IImpressionListener
    {
        private IImpressionsCache impressionsCache;

        public RedisTreatmentLog(IImpressionsCache impressionsCache)
        {
            this.impressionsCache = impressionsCache;
        }

        public void Log(KeyImpression impression)
        {
            impressionsCache.AddImpression(impression);
        }
    }
}