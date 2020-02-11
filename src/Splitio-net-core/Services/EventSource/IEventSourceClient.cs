using System;

namespace Splitio.Services.EventSource
{
    public interface IEventSourceClient
    {
        Status Status();
        void Close();

        event EventHandler<EventReceivedEventArgs> EventReceived;
        event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
    }
}
