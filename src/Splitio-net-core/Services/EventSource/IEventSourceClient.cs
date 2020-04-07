using System;

namespace Splitio.Services.EventSource
{
    public interface IEventSourceClient
    {
        void Connect();
        void Disconnect();
        bool IsConnected();
        
        event EventHandler<EventReceivedEventArgs> EventReceived;
        event EventHandler<EventArgs> ConnectedEvent;
        event EventHandler<EventArgs> DisconnectEvent;
    }
}
