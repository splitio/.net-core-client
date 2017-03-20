using log4net;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Cache.Classes
{
    public class InMemorySplitCache : ISplitCache
    {
        private static readonly ILog Log = LogManager.GetLogger("splitio",typeof(InMemorySplitCache));

        private ConcurrentDictionary<string, ParsedSplit> splits;
        private long changeNumber;

        public InMemorySplitCache(ConcurrentDictionary<string, ParsedSplit> splits, long changeNumber = -1)
        {
            this.splits = splits;
            this.changeNumber = changeNumber;
        }

        public void AddSplit(string splitName, SplitBase split)
        {
            splits.TryAdd(splitName, (ParsedSplit)split);
        }

        public bool RemoveSplit(string splitName)
        {
            ParsedSplit value;
            return splits.TryRemove(splitName, out value);
        }

        public void SetChangeNumber(long changeNumber)
        {
            if (changeNumber < this.changeNumber)
            {
                Log.Error("ChangeNumber for splits cache is less than previous");
            }
            this.changeNumber = changeNumber;
        }

        public long GetChangeNumber()
        {
            return changeNumber;
        }

        public SplitBase GetSplit(string splitName)
        {
            ParsedSplit value;
            splits.TryGetValue(splitName, out value);
            return value;
        }

        public List<SplitBase> GetAllSplits()
        {
            return splits.Values.ToList<SplitBase>(); 
        }
    }
}
