using Splitio.Services.Shared.Interfaces;

namespace Splitio.Services.Shared.Classes
{
    public class BlockUntilReadyService : IBlockUntilReadyService
    {
        public bool Ready { get; set; }

        public void BlockUntilReady(int blockMilisecondsUntilReady)
        {
            Ready = true;
        }

        public bool IsSdkReady()
        {
            return Ready;
        }
    }
}
