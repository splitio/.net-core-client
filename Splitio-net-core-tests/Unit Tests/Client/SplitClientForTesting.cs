using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio.Services.Evaluator;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ISplitLogger log, 
            ISplitCache splitCache,
            IAsynchronousListener<WrappedEvent> eventListener,
            IAsynchronousListener<IList<KeyImpression>> impressionListener,
            IBlockUntilReadyService blockUntilReadyService,
            IEvaluator evaluator)
            : base(log)
        {
            _splitCache = splitCache;
            _eventListener = eventListener;
            _impressionListener = impressionListener;
            _blockUntilReadyService = blockUntilReadyService;
            _trafficTypeValidator = new TrafficTypeValidator(_splitCache, log);
            _evaluator = evaluator;

            ApiKey = "SplitClientForTesting";
        }
    }
}
