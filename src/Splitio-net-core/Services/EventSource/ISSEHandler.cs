using System;

namespace Splitio.Services.EventSource
{
    public interface ISSEHandler
    {
        void Start(string token, string channels);
        void Stop();

        event EventHandler<FeedbackEventArgs> ConnectedEvent;
        event EventHandler<FeedbackEventArgs> DisconnectEvent;
    }
}
