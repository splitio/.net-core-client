using System;

namespace Splitio.Services.EventSource
{
    public interface IEventSourceClient
    {
        bool ConnectAsync(string url);
        void Disconnect(bool reconnect = false);
        bool IsConnected();
        
        event EventHandler<EventReceivedEventArgs> EventReceived;
        event EventHandler<FeedbackEventArgs> ConnectedEvent;
        event EventHandler<FeedbackEventArgs> DisconnectEvent;
        event EventHandler<EventArgs> ReconnectEvent;
    }
}
