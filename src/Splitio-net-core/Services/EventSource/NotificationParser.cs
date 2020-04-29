using Newtonsoft.Json;
using Splitio.Services.Exceptions;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationParser : INotificationParser
    {
        private const string _exceptionMessage = "Unexpected type received from EventSource";

        public IncomingNotification Parse(string notification)
        {
            if (notification.Contains("\"event\":\"message\""))
            {
                if (notification.Contains("[?occupancy=metrics.publishers]"))
                {
                    return ParseOccupancy(notification);
                }

                return ParseMessage(notification);
            }
            else if (notification.Contains("\"error\""))
            {
                return ParseError(notification);
            }

            throw new Exception(_exceptionMessage);
        }

        private IncomingNotification ParseMessage(string notificationString)
        {
            var result = new IncomingNotification();
            var notification = JsonConvert.DeserializeObject<Notification>(notificationString);
            var data = JsonConvert.DeserializeObject<IncomingNotification>(notification.Data.Data);

            switch (data?.Type)
            {
                case NotificationType.SPLIT_UPDATE:
                    result = JsonConvert.DeserializeObject<SplitChangeNotifiaction>(notification.Data.Data);
                    break;
                case NotificationType.SPLIT_KILL:
                    result = JsonConvert.DeserializeObject<SplitKillNotification>(notification.Data.Data);
                    break;
                case NotificationType.SEGMENT_UPDATE:
                    result = JsonConvert.DeserializeObject<SegmentChangeNotification>(notification.Data.Data);
                    break;
                case NotificationType.CONTROL:
                    result = JsonConvert.DeserializeObject<ControlNotification>(notification.Data.Data);
                    break;
                default:
                    throw new Exception(_exceptionMessage);
            }

            result.Channel = notification.Data.Channel;

            return result;
        }

        private IncomingNotification ParseOccupancy(string notificationString)
        {
            var notification = JsonConvert.DeserializeObject<Notification>(notificationString);

            var occupancyNotification = JsonConvert.DeserializeObject<OccupancyNotification>(notification.Data.Data);            

            if (occupancyNotification?.Metrics == null)
                throw new Exception(_exceptionMessage);

            occupancyNotification.Type = NotificationType.OCCUPANCY;
            occupancyNotification.Channel = notification.Data.Channel.Replace("[?occupancy=metrics.publishers]", string.Empty);

            return occupancyNotification;
        }

        private IncomingNotification ParseError(string notificationString)
        {
            var notificatinError = JsonConvert.DeserializeObject<NotificationError>(notificationString);

            if (notificatinError?.Error != null)
                throw new NotificationErrorException(notificatinError);

            throw new Exception(_exceptionMessage);
        }
    }
}
