﻿using Common.Logging;
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

        private readonly object _splitLock = new object();
        private readonly object _trafficTypeLock = new object();

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
            lock (_splitLock)
            {
                var isRemoved = RemoveSplit(splitName);

                AddSplit(splitName, split);

                return isRemoved;
            }
        }

        public void AddSplit(string splitName, SplitBase split)
        {
            lock (_splitLock)
            { 
                var parsedSplit = (ParsedSplit)split;

                if (_splits.TryAdd(splitName, parsedSplit))
                {
                    IncreaseTrafficTypeCount(parsedSplit.trafficTypeName);
                }
            }
        }

        public bool RemoveSplit(string splitName)
        {
            lock (_splitLock)
            {
                var removed = _splits.TryRemove(splitName, out ParsedSplit removedSplit);

                DecreaseTrafficTypeCount(removedSplit);

                return removed;
            }
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
            lock (_splitLock)
            {
                _splits.TryGetValue(splitName, out ParsedSplit value);

                return value;
            }
        }

        public List<SplitBase> GetAllSplits()
        {
            lock (_splitLock)
            {
                return _splits.Values.ToList<SplitBase>();
            }            
        }

        public void Clear()
        {
            lock (_splitLock)
            {
                _splits.Clear();
            }
            
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
            lock (_trafficTypeLock)
            {
                if (string.IsNullOrEmpty(trafficType)) return;

                if (_trafficTypes.TryGetValue(trafficType, out int quantity))
                {
                    var newValue = quantity + 1;

                    _trafficTypes.TryUpdate(trafficType, newValue, quantity);
                }
                else
                {
                    _trafficTypes.TryAdd(trafficType, 1);
                }
            }
        }

        private void DecreaseTrafficTypeCount(ParsedSplit split)
        {
            lock (_trafficTypeLock)
            {
                if (split != null && !string.IsNullOrEmpty(split.trafficTypeName))
                {
                    if (_trafficTypes.TryGetValue(split.trafficTypeName, out int quantity))
                    {
                        var newQuantity = quantity - 1;

                        _trafficTypes.TryUpdate(split.trafficTypeName, newQuantity, quantity);

                        if (newQuantity <= 0)
                        {
                            _trafficTypes.TryRemove(split.trafficTypeName, out int value);
                        }
                    }
                }
            }
        }
    }
}
