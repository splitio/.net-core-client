using System;

namespace Splitio.Services.EventSource
{
    public interface INotificationManagerKeeper
    {
        void HandleIncomingEvent(IncomingNotification notification);

        event EventHandler<SSEActionsEventArgs> ActionEvent;
    }
}
