using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Cache.Classes
{
    public class InMemorySplitCache : ISplitCache
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InMemorySplitCache));

        private ConcurrentDictionary<string, ParsedSplit> _splits;
        private ConcurrentDictionary<string, int> _trafficTypes;
        private long _changeNumber;

        public InMemorySplitCache(ConcurrentDictionary<string, ParsedSplit> splits, long changeNumber = -1)
        {
            _splits = splits;
            _changeNumber = changeNumber;
            _trafficTypes = new ConcurrentDictionary<string, int>();

            if (!splits.IsEmpty)
            {
                foreach (var split in splits)
                {
                    if (split.Value != null)
                    {
                        IncreaseTrafficTypeCount(split.Value.trafficTypeName);
                    }
                }
            }
        }

        public bool AddOrUpdate(string splitName, SplitBase split)
        {
            var parsedSplit = (ParsedSplit)split;

            var exists = _splits.TryGetValue(splitName, out ParsedSplit oldSplit);

            if (exists)
            {
                DecreaseTrafficTypeCount(oldSplit);
            }

            _splits.AddOrUpdate(splitName, parsedSplit, (key, oldValue) => parsedSplit);
            
            IncreaseTrafficTypeCount(parsedSplit.trafficTypeName);

            return exists;
        }

        public void AddSplit(string splitName, SplitBase split)
        {
            var parsedSplit = (ParsedSplit)split;

            if (_splits.TryAdd(splitName, parsedSplit))
            {
                IncreaseTrafficTypeCount(parsedSplit.trafficTypeName);
            }
        }

        public bool RemoveSplit(string splitName)
        {            
            var removed = _splits.TryRemove(splitName, out ParsedSplit removedSplit);

            if (removed)
            {
                DecreaseTrafficTypeCount(removedSplit);
            }            

            return removed;
        }

        public void SetChangeNumber(long changeNumber)
        {
            if (changeNumber < _changeNumber)
            {
                Log.Error("ChangeNumber for splits cache is less than previous");
            }

            _changeNumber = changeNumber;
        }

        public long GetChangeNumber()
        {
            return _changeNumber;
        }

        public SplitBase GetSplit(string splitName)
        {
            _splits.TryGetValue(splitName, out ParsedSplit value);

            return value;
        }

        public List<SplitBase> GetAllSplits()
        {            
            return _splits.Values.ToList<SplitBase>();            
        }

        public void Clear()
        {
            _splits.Clear();            
            _trafficTypes.Clear();
        }

        public bool TrafficTypeExists(string trafficType)
        {
            var quantity = 0;

            var exists = string.IsNullOrEmpty(trafficType) 
                ? false 
                : _trafficTypes.TryGetValue(trafficType, out quantity);

            return exists && quantity > 0;
        }

        private void IncreaseTrafficTypeCount(string trafficType)
        {
            if (string.IsNullOrEmpty(trafficType)) return;

            _trafficTypes.AddOrUpdate(trafficType, 1, (key, oldValue) => oldValue++);
        }

        private void DecreaseTrafficTypeCount(ParsedSplit split)
        {
            if (split == null || string.IsNullOrEmpty(split.trafficTypeName)) return;
            
            if (_trafficTypes.TryGetValue(split.trafficTypeName, out int quantity))
            {
                if (quantity <= 1)
                {
                    _trafficTypes.TryRemove(split.trafficTypeName, out int value);

                    return;
                }

                var newQuantity = quantity - 1;

                _trafficTypes.TryUpdate(split.trafficTypeName, newQuantity, quantity);
            }
        }
    }
}
