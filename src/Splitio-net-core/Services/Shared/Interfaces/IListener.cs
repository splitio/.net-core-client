namespace Splitio.Services.Shared.Interfaces
{
    public interface IListener<T>
    {
        void Log(T item);
    }
}
