using System.Collections.Generic;

namespace Splitio.Services.Cache.Interfaces
{
    public interface ISimpleCache<T>
    {
        void AddItem(T item);

        List<T> FetchAllAndClear();

        bool HasReachedMaxSize();
    }
}
