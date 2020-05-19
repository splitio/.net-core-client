using System;

namespace Splitio.Services.EventSource
{
    public interface INotificationManagerKeeper
    {
        void HandleIncomingEvent(IncomingNotification notification);

        event EventHandler<OccupancyEventArgs> OccupancyEvent;
        event EventHandler<EventArgs> PushShutdownEvent;
    }
}
