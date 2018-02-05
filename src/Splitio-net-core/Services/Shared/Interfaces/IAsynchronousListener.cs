namespace Splitio.Services.Shared.Interfaces
{
    public interface IAsynchronousListener<T> : IListener<T>
    {
        void AddListener(IListener<T> worker);
    }
}
