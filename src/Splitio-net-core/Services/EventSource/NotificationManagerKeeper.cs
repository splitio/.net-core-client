using Splitio.Domain;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationManagerKeeper : INotificationManagerKeeper
    {
        private readonly ISplitLogger _log;
        private readonly object _eventOccupancyLock = new object();

        private bool _publisherAvailable;
        private int _publishersPri;
        private int _publishersSec;

        public event EventHandler<OccupancyEventArgs> OccupancyEvent;
        public event EventHandler<EventArgs> PushShutdownEvent;

        public NotificationManagerKeeper(ISplitLogger log = null)
        {
            _log = log ?? WrapperAdapter.GetLogger(typeof(NotificationManagerKeeper));

            _publisherAvailable = true;
        }

        #region Public Methods
        public void HandleIncomingEvent(IncomingNotification notification)
        {
            switch (notification.Type)
            {
                case NotificationType.CONTROL:
                    ProcessEventControl(notification);
                    break;
                case NotificationType.OCCUPANCY:
                    ProcessEventOccupancy(notification);
                    break;
                default:
                    _log.Error($"Incorrect notification type: {notification.Type}");
                    break;
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
                    lock (_eventOccupancyLock)
                    {
                        if (_publisherAvailable) DispatchOccupancyEvent(publisherAvailable: true);
                    }
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
            lock (_eventOccupancyLock)
            {
                var occupancyEvent = (OccupancyNotification)notification;

                UpdatePublishers(occupancyEvent.Channel, occupancyEvent.Metrics.Publishers);

                if (!ArePublishersAvailable() && _publisherAvailable)
                {
                    _publisherAvailable = false;
                    DispatchOccupancyEvent(false);
                }
                else if (ArePublishersAvailable() && !_publisherAvailable)
                {
                    _publisherAvailable = true;
                    DispatchOccupancyEvent(true);
                }
            }
        }

        private void UpdatePublishers(string channel, int publishers)
        {
            if (channel.Equals(Constans.PushControlPri))
            {
                _publishersPri = publishers;
                return;
            }

            if (channel.Equals(Constans.PushControlSec))
            {
                _publishersSec = publishers;
                return;
            }
        }

        private bool ArePublishersAvailable()
        {
            return _publishersPri >= 1 || _publishersSec >= 1;
        }

        private void DispatchOccupancyEvent(bool publisherAvailable)
        {
            OccupancyEvent?.Invoke(this, new OccupancyEventArgs(publisherAvailable));
        }

        private void DispatchPushShutdown()
        {
            PushShutdownEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
