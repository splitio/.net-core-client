namespace Splitio.Services.EventSource
{
    public interface INotificationPorcessor
    {
        void StartClient();
        void StopClient();
    }
}
