using Splitio.Domain;
using Splitio.Services.Cache.Lru;
using Splitio.Services.Impressions.Interfaces;
using System;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionsObserver : IImpressionsObserver
    {
        private const int DefaultCacheSize = 500000;
        
        private readonly LruCache<ulong, long> _cache;
        private readonly IImpressionHasher _impressionHasher;

        public ImpressionsObserver(IImpressionHasher impressionHasher)
        {
            _impressionHasher = impressionHasher;

            _cache = new LruCache<ulong, long>(DefaultCacheSize);            
        }

        public long? TestAndSet(KeyImpression impression)
        {
            long? defaultReturn = null;

            if (impression == null)
            {
                return defaultReturn;
            }

            ulong hash = _impressionHasher.Process(impression);

            try
            {
                long? previous = _cache.Get(hash);
                _cache.AddOrUpdate(hash, impression.time);

                return Math.Min(previous.Value, impression.time);
            }
            catch (Exception)
            {
                _cache.AddOrUpdate(hash, impression.time);

                return defaultReturn;
            }
        }
    }
}
