using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ILog _log, 
            ISplitCache _splitCache, 
            Splitter _splitter, 
            IListener<WrappedEvent> _eventListener) 
            : base(_log)
        {
            splitCache = _splitCache;
            splitter = _splitter;
            eventListener = _eventListener;

            _trafficTypeValidator = new TrafficTypeValidator(_log, _splitCache);
        }

        public override void Destroy() { }
    }
}
