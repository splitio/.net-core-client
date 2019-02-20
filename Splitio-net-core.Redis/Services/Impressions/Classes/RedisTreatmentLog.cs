using Splitio.Domain;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;

namespace Splitio.Redis.Services.Impressions.Classes
{
    public class RedisTreatmentLog : IListener<IList<KeyImpression>>
    {
        private ISimpleCache<IList<KeyImpression>> impressionsCache;

        public RedisTreatmentLog(ISimpleCache<IList<KeyImpression>> impressionsCache)
        {
            this.impressionsCache = impressionsCache;
        }

        public void Log(IList<KeyImpression> items)
        {
            impressionsCache.AddItem(items);
        }
    }
}