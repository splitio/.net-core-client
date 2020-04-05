namespace Splitio.Services.Common
{
    public interface IPushManager
    {
        void StartSse();
        void StopSse();
    }
}
