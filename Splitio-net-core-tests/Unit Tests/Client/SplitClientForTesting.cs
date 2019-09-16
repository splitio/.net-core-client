using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio.Services.Evaluator;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ILog _log, 
            ISplitCache _splitCache, 
            IListener<WrappedEvent> _eventListener,
            IListener<KeyImpression> _impressionListener,
            IBlockUntilReadyService blockUntilReadyService,
            IEvaluator evaluator)
            : base(_log)
        {
            splitCache = _splitCache;
            eventListener = _eventListener;
            impressionListener = _impressionListener;
            _blockUntilReadyService = blockUntilReadyService;
            _trafficTypeValidator = new TrafficTypeValidator(_log, _splitCache);
            _evaluator = evaluator;

            ApiKey = "SplitClientForTesting";
        }
    }
}
