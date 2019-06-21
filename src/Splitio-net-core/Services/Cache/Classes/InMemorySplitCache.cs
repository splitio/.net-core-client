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
                        AddTrafficType(split.Value.trafficTypeName);
                    }
                }
            }
        }

        public void AddSplit(string splitName, SplitBase split)
        {
            var parsedSplit = (ParsedSplit)split;

            _splits.TryAdd(splitName, parsedSplit);

            AddTrafficType(parsedSplit.trafficTypeName);
        }

        public bool RemoveSplit(string splitName)
        {
            var removed = _splits.TryRemove(splitName, out ParsedSplit removedSplit);

            RemoveTrafficType(removedSplit);

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
            ParsedSplit value;
            _splits.TryGetValue(splitName, out value);
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

        private void AddTrafficType(string trafficType)
        {
            if (string.IsNullOrEmpty(trafficType)) return;
            
            var ttExists = _trafficTypes.TryGetValue(trafficType, out int quantity);

            if (ttExists)
            {
                _trafficTypes.TryUpdate(trafficType, quantity++, quantity);
            }
            else
            {
                _trafficTypes.TryAdd(trafficType, 1);
            }            
        }

        private void RemoveTrafficType(ParsedSplit split)
        {
            if (split != null && !string.IsNullOrEmpty(split.trafficTypeName))
            {
                var ttExists = _trafficTypes.TryGetValue(split.trafficTypeName, out int quantity);

                if (ttExists)
                {
                    var newQuantity = quantity--;
                    _trafficTypes.TryUpdate(split.trafficTypeName, newQuantity, quantity);

                    if (newQuantity <= 0) _trafficTypes.TryRemove(split.trafficTypeName, out int value);
                }
            }
        }
    }
}
