using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Impressions.Classes;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Cache.Classes
{
    public class InMemoryImpressionsCache : IImpressionsCache
    {
        private BlockingQueue<KeyImpression> queue;

        public InMemoryImpressionsCache(BlockingQueue<KeyImpression> queue)
        {
            this.queue = queue;
        }

        public void AddImpression(KeyImpression impression)
        {
            if (queue != null)
            {
                queue.Enqueue(impression);
            }
        }

        public List<KeyImpression> FetchAllAndClear()
        {
            return queue != null ? queue.FetchAllAndClear().ToList() : null;
        }

        public bool HasReachedMaxSize()
        {
            return queue != null ? queue.HasReachedMaxSize() : false;
        }
    }
}
