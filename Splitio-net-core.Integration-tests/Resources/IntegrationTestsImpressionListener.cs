using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Shared.Classes;

namespace Splitio_net_core.Integration_tests.Resources
{
    public class IntegrationTestsImpressionListener : IImpressionListener
    {
        BlockingQueue<KeyImpression> queue;

        public IntegrationTestsImpressionListener(int size)
        {
            queue = new BlockingQueue<KeyImpression>(size);
        }

        public void Log(KeyImpression impression)
        {
            if (queue.HasReachedMaxSize())
            {
                queue.Dequeue();
            }

            queue.Enqueue(impression);            
        }

        public BlockingQueue<KeyImpression> GetQueue()
        {
            return queue;
        }
    }
}
