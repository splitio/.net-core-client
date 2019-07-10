namespace Splitio.Services.Shared.Interfaces
{
    public interface IBlockUntilReadyService
    {
        void BlockUntilReady(int blockMilisecondsUntilReady);
        bool IsSdkReady();
    }
}