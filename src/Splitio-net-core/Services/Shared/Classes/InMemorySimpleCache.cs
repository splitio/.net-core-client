using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Shared.Classes
{
    public class InMemorySimpleCache<T> : ISimpleCache<T>
    {
        private BlockingQueue<T> queue;

        public InMemorySimpleCache(BlockingQueue<T> queue)
        {
            this.queue = queue;
        }

        public void AddItem(T item)
        {
            if (queue != null)
            {
                queue.Enqueue(item);
            }
        }

        public List<T> FetchAllAndClear()
        {
            return queue != null ? queue.FetchAllAndClear().ToList() : null;
        }

        public bool HasReachedMaxSize()
        {
            return queue != null ? queue.HasReachedMaxSize() : false;
        }
    }
}
