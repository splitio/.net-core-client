using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class LocalhostClientForTesting : LocalhostClient
    {
        public LocalhostClientForTesting(string filePath,
            ISplitLogger log = null,
            ISplitter splitter = null,
            bool isDestroyed = false) : base(filePath, log)
        {
            Destroyed = isDestroyed;
        }

        //public IAsynchronousListener<WrappedEvent> GetEventListener()
        //{
        //    return _eventListener;
        //}
    }
}
