using System;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource
{
    public interface IEventSourceClient
    {
        Task ConnectAsync(string url);
        void Disconnect(bool reconnect = false);
        bool IsConnected();
        
        event EventHandler<EventReceivedEventArgs> EventReceived;
        event EventHandler<FeedbackEventArgs> ConnectedEvent;
        event EventHandler<FeedbackEventArgs> DisconnectEvent;
    }
}
