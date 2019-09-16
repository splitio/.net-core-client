using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class LocalhostClientForTesting : LocalhostClient
    {
        public LocalhostClientForTesting(string filePath,
            ILog log,
            ISplitter splitter = null,
            bool isDestroyed = false) : base(filePath, log)
        {
            Destroyed = isDestroyed;
        }

        public IListener<WrappedEvent> GetEventListener()
        {
            return eventListener;
        }
    }
}
