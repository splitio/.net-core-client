using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class LocalhostClientForTesting : LocalhostClient
    {
        public LocalhostClientForTesting(string filePath, Splitter splitter = null) : base(filePath, splitter) { }

        public IListener<Event> GetEventListener()
        {
            return base.eventListener;
        }
    }
}
