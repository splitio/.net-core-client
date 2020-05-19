namespace Splitio.Services.Common
{
    public interface IBackOff
    {
        double GetInterval();
        void Reset();
        int GetAttempt();
    }
}
