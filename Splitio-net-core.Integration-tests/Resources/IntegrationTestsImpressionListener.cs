using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Shared.Classes;
using System.Collections.Generic;

namespace Splitio_net_core.Integration_tests.Resources
{
    public class IntegrationTestsImpressionListener : IImpressionListener
    {
        BlockingQueue<KeyImpression> queue;

        public IntegrationTestsImpressionListener(int size)
        {
            queue = new BlockingQueue<KeyImpression>(size);
        }

        public void Log(IList<KeyImpression> impressions)
        {
            if (queue.HasReachedMaxSize())
            {
                queue.Dequeue();
            }

            foreach (var imp in impressions)
            {
                queue.Enqueue(imp);
            }            
        }

        public BlockingQueue<KeyImpression> GetQueue()
        {
            return queue;
        }
    }
}
