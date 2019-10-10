using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.Shared.Interfaces;
using Splitio.Services.SplitFetcher.Classes;
using System;

namespace Splitio.Services.Shared.Classes
{
    public class SelfRefreshingBlockUntilReadyService : IBlockUntilReadyService
    {
        private readonly SelfRefreshingSplitFetcher _splitFetcher;
        private readonly SelfRefreshingSegmentFetcher _selfRefreshingSegmentFetcher;
        private readonly IReadinessGatesCache _gates;        
        private readonly IListener<KeyImpression> _treatmentLog;
        private readonly IListener<WrappedEvent> _eventLog;
        private readonly ISplitLogger _log;

        public SelfRefreshingBlockUntilReadyService(IReadinessGatesCache gates,
            SelfRefreshingSplitFetcher splitFetcher,
            SelfRefreshingSegmentFetcher selfRefreshingSegmentFetcher,
            IListener<KeyImpression> treatmentLog,
            IListener<WrappedEvent> eventLog, 
            ISplitLogger log = null)
        {
            _gates = gates;
            _splitFetcher = splitFetcher;
            _selfRefreshingSegmentFetcher = selfRefreshingSegmentFetcher;
            _treatmentLog = treatmentLog;
            _eventLog = eventLog;
            _log = log ?? WrapperAdapter.GetLogger(typeof(SelfRefreshingBlockUntilReadyService));
        }

        public void BlockUntilReady(int blockMilisecondsUntilReady)
        {
            if (!IsSdkReady())
            {
                if (blockMilisecondsUntilReady <= 0)
                {
                    _log.Warn("The blockMilisecondsUntilReady param has to be higher than 0.");
                }
                
                if (!_gates.IsSDKReady(blockMilisecondsUntilReady))
                {
                    throw new TimeoutException(string.Format($"SDK was not ready in {blockMilisecondsUntilReady} miliseconds"));
                }
            }
        }

        public bool IsSdkReady()
        {
            return _gates.IsSDKReady(0);
        }
    }
}
