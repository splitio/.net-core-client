using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Cache.Interfaces
{
    public interface ISplitCache
    {
        void AddSplit(string splitName, SplitBase split);

        bool RemoveSplit(string splitName);

        bool AddOrUpdate(string splitName, SplitBase split);

        void SetChangeNumber(long changeNumber);

        long GetChangeNumber();

        ParsedSplit GetSplit(string splitName);

        List<ParsedSplit> GetAllSplits();

        void Clear();

        bool TrafficTypeExists(string trafficType);
    }
}
