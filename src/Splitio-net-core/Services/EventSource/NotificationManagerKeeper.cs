using Splitio.Domain;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationManagerKeeper : INotificationManagerKeeper
    {
        private readonly ISplitLogger _log;

        private readonly object _publisherAvailableLock = new object();
        private bool _publisherAvailable;

        public event EventHandler<OccupancyEventArgs> OccupancyEvent;
        public event EventHandler<EventArgs> PushShutdownEvent;

        public NotificationManagerKeeper(ISplitLogger log = null)
        {
            _log = log ?? WrapperAdapter.GetLogger(typeof(NotificationManagerKeeper));

            UpdatePublisherAvailable(publisherAvailable: true);
        }

        #region Public Methods
        public void HandleIncomingEvent(IncomingNotification notification)
        {
            if (notification.Type == NotificationType.CONTROL)
            {
                ProcessEventControl(notification);
            }
            else if (notification.Type == NotificationType.OCCUPANCY && notification.Channel == Constans.PushControlPri)
            {
                ProcessEventOccupancy(notification);
            }
        }
        #endregion

        #region Private Methods
        private void ProcessEventControl(IncomingNotification notification)
        {
            var controlEvent = (ControlNotification)notification;

            switch (controlEvent.ControlType)
            {
                case ControlType.STREAMING_PAUSED:
                    DispatchOccupancyEvent(publisherAvailable: false);
                    break;
                case ControlType.STREAMING_RESUMED:
                    if (IsPublisherAvailable()) DispatchOccupancyEvent(publisherAvailable: true);
                    break;
                case ControlType.STREAMING_DISABLED:
                    DispatchPushShutdown();
                    break;
                default:
                    _log.Error($"Incorrect control type. {controlEvent.ControlType}");
                    break;
            }
        }

        private void ProcessEventOccupancy(IncomingNotification notification)
        {
            var occupancyEvent = (OccupancyNotification)notification;

            if (occupancyEvent.Metrics.Publishers <= 0 && IsPublisherAvailable())
            {
                UpdatePublisherAvailable(publisherAvailable: false);
                DispatchOccupancyEvent(false);
            }
            else if (occupancyEvent.Metrics.Publishers >= 1 && !IsPublisherAvailable())
            {
                UpdatePublisherAvailable(publisherAvailable: true);
                DispatchOccupancyEvent(true);
            }
        }

        private void DispatchOccupancyEvent(bool publisherAvailable)
        {
            OccupancyEvent?.Invoke(this, new OccupancyEventArgs(publisherAvailable));
        }

        private void DispatchPushShutdown()
        {
            PushShutdownEvent?.Invoke(this, EventArgs.Empty);
        }

        public bool IsPublisherAvailable()
        {
            lock (_publisherAvailableLock)
            {
                return _publisherAvailable;
            }
        }

        private void UpdatePublisherAvailable(bool publisherAvailable)
        {
            lock (_publisherAvailableLock)
            {
                _publisherAvailable = publisherAvailable;
            }
        }
        #endregion
    }
}
