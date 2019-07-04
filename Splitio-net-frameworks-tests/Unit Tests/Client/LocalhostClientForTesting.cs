using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_net_frameworks_tests.Unit_Tests.Client
{
    public class LocalhostClientForTesting : LocalhostClient
    {
        public LocalhostClientForTesting(string filePath, 
            ILog log, 
            IFactoryInstantiationsService factoryInstantiationsService, 
            Splitter splitter = null,
            bool isDestroyed = false) : base(filePath, log, factoryInstantiationsService, splitter)
        {
            Destroyed = isDestroyed;
        }

        public IListener<WrappedEvent> GetEventListener()
        {
            return eventListener;
        }
    }
}   