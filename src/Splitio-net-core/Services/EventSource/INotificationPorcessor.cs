namespace Splitio.Services.EventSource
{
    public interface INotificationPorcessor
    {
        void Proccess(IncomingNotification notification);
    }
}
