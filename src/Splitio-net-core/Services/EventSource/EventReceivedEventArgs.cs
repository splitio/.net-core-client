using System;

namespace Splitio.Services.EventSource
{
    public class EventReceivedEventArgs : EventArgs
    {
        public EventData Event { get; }

        public EventReceivedEventArgs(EventData eventReceived)
        {
            Event = eventReceived;
        }
    }
}
