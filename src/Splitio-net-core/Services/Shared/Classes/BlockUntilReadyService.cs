using Splitio.Services.Shared.Interfaces;

namespace Splitio.Services.Shared.Classes
{
    public class BlockUntilReadyService : IBlockUntilReadyService
    {
        public void BlockUntilReady(int blockMilisecondsUntilReady) { }

        public bool IsSdkReady()
        {
            return true;
        }
    }
}
