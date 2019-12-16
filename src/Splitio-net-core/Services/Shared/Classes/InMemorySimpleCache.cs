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

        public void AddItems(IList<T> items)
        {
            if (_queue != null)
            {
                foreach (var item in items)
                {
                    _queue.Enqueue(item);
                }
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
