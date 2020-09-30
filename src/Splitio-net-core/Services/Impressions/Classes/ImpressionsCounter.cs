using Splitio.Services.Impressions.Interfaces;
using System.Collections.Concurrent;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionsCounter : IImpressionsCounter
    {
        private const int DefaultAmount = 1;


        private readonly ConcurrentDictionary<string, int> _cache;

        public ImpressionsCounter()
        {
            _cache = new ConcurrentDictionary<string, int>();
        }

        public void Inc(string splitName, long timeFrame)
        {
            var key = $"{splitName}::{ImpressionsHelper.TruncateTimeFrame(timeFrame)}";

            _cache.AddOrUpdate(key, DefaultAmount, (keyCache, cacheAmount) => cacheAmount + DefaultAmount);
        }

        public ConcurrentDictionary<string, int> PopAll()
        {
            var values = new ConcurrentDictionary<string, int>(_cache);

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
    }
}
