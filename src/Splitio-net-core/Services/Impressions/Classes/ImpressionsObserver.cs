using Splitio.Domain;
using Splitio.Services.Cache.Lru;
using System;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionsObserver
    {
        private readonly LruCache<long, long> _cache;

        public ImpressionsObserver()
        {
            _cache = new LruCache<long, long>(5000);
        }

        public long? TestAndSet(KeyImpression impression)
        {
            long? defaultValue = null;

            if (impression == null)
            {
                return defaultValue;
            }

            long hash = 9827492;
            long? previous = _cache.Get(hash);

            _cache.AddOrUpdate(hash, impression.time);

            return previous == null ? defaultValue : Math.Min(previous.Value, impression.time);
        }
    }
}
