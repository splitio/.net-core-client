using System.Collections.Generic;

namespace Splitio.Services.Shared.Interfaces
{
    public interface ISimpleCache<T>
    {
        void AddItems(IList<T> items);
    }
}
