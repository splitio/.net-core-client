using System.Collections.Generic;

namespace Splitio.Services.Cache.Interfaces
{
    public interface ITrafficTypesCache
    {
        void Load(List<string> trafficTypes);
        void Clear();
        bool Exists(string trafficType);
    }
}
