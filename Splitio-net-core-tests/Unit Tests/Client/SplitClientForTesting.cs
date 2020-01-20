using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio.Services.Evaluator;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ISplitLogger log, 
            ISplitCache splitCache,
            IEventsLog eventsLog,
            IImpressionsLog impressionsLog,
            IBlockUntilReadyService blockUntilReadyService,
            IEvaluator evaluator)
            : base(log)
        {
            _splitCache = splitCache;
            _eventsLog = eventsLog;
            _impressionsLog = impressionsLog;
            _blockUntilReadyService = blockUntilReadyService;
            _trafficTypeValidator = new TrafficTypeValidator(_splitCache, log);
            _evaluator = evaluator;

            ApiKey = "SplitClientForTesting";
        }
    }
}
