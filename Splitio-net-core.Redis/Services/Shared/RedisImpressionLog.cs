using System.Collections.Generic;
using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Shared.Interfaces;

namespace Splitio.Redis.Services.Shared
{
    public class RedisImpressionLog : IImpressionsLog
    {
        private readonly ISimpleCache<KeyImpression> _impressionsCache;

        public RedisImpressionLog(ISimpleCache<KeyImpression> impressionsCache)
        {
            _impressionsCache = impressionsCache;
        }

        public void AddItems(IList<KeyImpression> impressions)
        {
            _impressionsCache.AddItems(impressions);
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}
