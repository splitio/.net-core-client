using Splitio.Services.Cache.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Cache.Classes
{
    public class InMemoryTrafficTypesCache : ITrafficTypesCache
    {
        private ConcurrentBag<string> _trafficTypes;

        public InMemoryTrafficTypesCache()
        {
            _trafficTypes = new ConcurrentBag<string>();
        }

        public void Load(List<string> trafficTypes)
        {
            foreach (var type in trafficTypes)
            {
                if (!Exists(type))
                    _trafficTypes.Add(type);
            }            
        }

        public void Clear()
        {
            _trafficTypes = new ConcurrentBag<string>();
        }

        public bool Exists(string trafficType)
        {
            return _trafficTypes.FirstOrDefault(tt => trafficType.Equals(tt)) != null;
        }
    }
}
