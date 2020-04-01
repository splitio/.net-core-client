namespace Splitio.Services.EventSource
{
    public interface INotificationParser
    {
        EventData Parse(string text);
    }
}
