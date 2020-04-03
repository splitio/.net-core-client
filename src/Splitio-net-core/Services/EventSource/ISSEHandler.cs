namespace Splitio.Services.EventSource
{
    public interface ISSEHandler
    {
        void Start();
        void Stop();
        void StartWorkers();
        void StopWorkers();
    }
}
