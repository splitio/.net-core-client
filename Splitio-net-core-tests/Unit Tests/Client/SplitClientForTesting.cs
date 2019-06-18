using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ILog log, ISplitCache _splitCache, Splitter _splitter, IListener<WrappedEvent> eventListener) 
            : base(log)
        {
            splitCache = _splitCache;
            splitter = _splitter;
            this.eventListener = eventListener;
        }

        public override void Destroy()
        {
        }
    }
}
