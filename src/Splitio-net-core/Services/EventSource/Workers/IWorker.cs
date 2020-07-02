namespace Splitio.Services.EventSource.Workers
{
    public interface IWorker<T>
    {
        void AddToQueue(T element);
        void Start();
        void Stop();
    }
}
