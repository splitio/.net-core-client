using Splitio.Services.Impressions.Interfaces;
using System;
using System.Collections.Concurrent;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionsCounter : IImpressionsCounter
    {
        private const int DefaultAmount = 1;

        private readonly ConcurrentDictionary<KeyCache, int> _cache;

        public ImpressionsCounter()
        {
            _cache = new ConcurrentDictionary<KeyCache, int>();
        }

        public void Inc(string splitName, long timeFrame)
        {
            var key = new KeyCache(splitName, timeFrame);

            _cache.AddOrUpdate(key, DefaultAmount, (keyCache, cacheAmount) => cacheAmount + DefaultAmount);
        }

        public ConcurrentDictionary<KeyCache, int> PopAll()
        {
            var values = new ConcurrentDictionary<KeyCache, int>(_cache);

            _cache.Clear();

            return values;
        }
    }

    public class KeyCache
    {
        public string SplitName { get; set; }
        public long TimeFrame { get; set; }

        public KeyCache(string splitName, long timeFrame)
        {
            SplitName = splitName;
            TimeFrame = ImpressionsHelper.TruncateTimeFrame(timeFrame);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(SplitName, TimeFrame).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || obj.GetType() != GetType()) return false;

            var key = (KeyCache)obj;
            return key.SplitName.Equals(SplitName) && key.TimeFrame.Equals(TimeFrame);
        }
    }
}
