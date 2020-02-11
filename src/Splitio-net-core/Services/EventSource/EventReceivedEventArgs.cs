using System;

namespace Splitio.Services.EventSource
{
    public class EventReceivedEventArgs : EventArgs
    {
        public Event Event { get; }

        public EventReceivedEventArgs(Event eventReceived)
        {
            Event = eventReceived;
        }
    }
}
