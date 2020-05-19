namespace Splitio.Services.EventSource
{
    public interface INotificationParser
    {
        IncomingNotification Parse(string notificationString);
    }
}
