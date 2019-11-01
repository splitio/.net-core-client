using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio.Services.Evaluator;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ISplitLogger _log, 
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
            _trafficTypeValidator = new TrafficTypeValidator(_splitCache, _log);
            _evaluator = evaluator;

            ApiKey = "SplitClientForTesting";
        }
    }
}
