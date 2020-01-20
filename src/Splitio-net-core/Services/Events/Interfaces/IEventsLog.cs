using Splitio.Domain;

namespace Splitio.Services.Events.Interfaces
{
    public interface IEventsLog
    {
        void Start();
        void Stop();
        void Log(WrappedEvent wrappedEvent);
    }
}
