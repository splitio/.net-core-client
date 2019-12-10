namespace Splitio.Services.Shared.Interfaces
{
    public interface IAsynchronousListener<T>
    {
        void AddListener(IListener<T> worker);
        void Notify(T item);
    }
}
