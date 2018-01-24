using System.Collections.Generic;

namespace Splitio.Services.Shared.Interfaces
{
    public interface ISimpleProducerCache<T> : ISimpleCache<T>
    {
        List<T> FetchAllAndClear();
        bool HasReachedMaxSize();
    }
}
