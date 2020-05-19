using System;

namespace Splitio.Services.EventSource
{
    public class OccupancyEventArgs : EventArgs
    {
        public bool PublisherAvailable { get; }

        public OccupancyEventArgs(bool publisherAvailable)
        {
            PublisherAvailable = publisherAvailable;
        }
    }
}
