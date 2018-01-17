namespace Splitio.Services.Events.Interfaces
{
    public interface IEventSdkApiClient
    {
        void SendBulkEvents(string events);
    }
}
