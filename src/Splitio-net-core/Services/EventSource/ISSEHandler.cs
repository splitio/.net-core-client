namespace Splitio.Services.EventSource
{
    public interface ISSEHandler
    {
        void Start(string token, string channels);
        void Stop();
    }
}
