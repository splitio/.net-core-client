using Splitio.Services.SegmentFetcher.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Splitio.Domain
{
    public class Segment: ISegment
    {
        private string name;
        public long changeNumber { get; set; }
        //ConcurrentDictionary with no value is the concurrent alternative for HashSet
        private ConcurrentDictionary<string, byte> keys;

        public Segment(string name, long changeNumber = -1, ConcurrentDictionary<string, byte> keys= null)
        {
            this.name = name;
            this.changeNumber = changeNumber;
            this.keys = keys ?? new ConcurrentDictionary<string, byte>();
        }

        public void AddKeys(List<string> keys)
        {
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    this.keys.TryAdd(key, 0);
                }
            }
        }

        public void RemoveKeys(List<string> keys)
        {
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    byte value;
                    this.keys.TryRemove(key, out value);
                }
            }
        }

        public bool Contains(string key)
        {
            byte value;
            return keys.TryGetValue(key, out value);
        }
    }
}
