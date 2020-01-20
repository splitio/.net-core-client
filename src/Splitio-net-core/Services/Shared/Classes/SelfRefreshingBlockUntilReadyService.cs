using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;
using System;

namespace Splitio.Services.Shared.Classes
{
    public class SelfRefreshingBlockUntilReadyService : IBlockUntilReadyService
    {
        private readonly IReadinessGatesCache _gates;
        private readonly ISplitLogger _log;

        public SelfRefreshingBlockUntilReadyService(IReadinessGatesCache gates,
            ISplitLogger log = null)
        {
            _gates = gates;
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
