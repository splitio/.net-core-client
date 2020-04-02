using System;

namespace Splitio.Services.EventSource
{
    public class EventReceivedEventArgs : EventArgs
    {
        public IncomingNotification Event { get; }

        public EventReceivedEventArgs(IncomingNotification incomingNotification)
        {
            Event = incomingNotification;
        }
    }
}
