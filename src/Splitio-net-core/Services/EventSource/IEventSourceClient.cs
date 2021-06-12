using System;

namespace Splitio.Services.EventSource
{
    public interface IEventSourceClient
    {
        bool ConnectAsync(string url);
        void Disconnect(SSEClientActions action = SSEClientActions.DISCONNECT);
        bool IsConnected();
        
        event EventHandler<EventReceivedEventArgs> EventReceived;
        event EventHandler<SSEActionsEventArgs> ActionEvent;
    }
}
