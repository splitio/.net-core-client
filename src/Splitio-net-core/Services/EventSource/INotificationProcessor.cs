namespace Splitio.Services.EventSource
{
    public interface INotificationProcessor
    {
        void Proccess(IncomingNotification notification);
    }
}
