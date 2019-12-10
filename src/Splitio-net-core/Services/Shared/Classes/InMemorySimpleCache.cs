using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Shared.Classes
{
    public class InMemorySimpleCache<T> : ISimpleProducerCache<T>
    {
        private readonly BlockingQueue<T> _queue;

        public InMemorySimpleCache(BlockingQueue<T> queue)
        {
            _queue = queue;
        }

        public void AddItem(T item)
        {
            if (_queue != null)
            {
                _queue.Enqueue(item);
            }
        }

        public List<T> FetchAllAndClear()
        {
            return _queue != null ? _queue.FetchAllAndClear().ToList() : null;
        }

        public bool HasReachedMaxSize()
        {
            return _queue != null ? _queue.HasReachedMaxSize() : false;
        }
    }
}
