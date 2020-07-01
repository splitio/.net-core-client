using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Services.Exceptions;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationParser : INotificationParser
    {
        private const string _exceptionMessage = "Unexpected type received from EventSource";

        public IncomingNotification Parse(string notification)
        {
            if (notification.Contains("event: message"))
            {
                if (notification.Contains(Constans.PushOccupancyPrefix))
                {
                    return ParseControlChannelMessage(notification);
                }

                return ParseMessage(notification);
            }
            else if (notification.Contains("event: error"))
            {
                return ParseError(notification);
            }

            return null;
        }

        private IncomingNotification ParseMessage(string notificationString)
        {
            var result = new IncomingNotification();

            var notificationData = GetNotificationData<NotificationData>(notificationString);
            var data = JsonConvert.DeserializeObject<IncomingNotification>(notificationData.Data);

            switch (data?.Type)
            {
                case NotificationType.SPLIT_UPDATE:
                    result = JsonConvert.DeserializeObject<SplitChangeNotifiaction>(notificationData.Data);
                    break;
                case NotificationType.SPLIT_KILL:
                    result = JsonConvert.DeserializeObject<SplitKillNotification>(notificationData.Data);
                    break;
                case NotificationType.SEGMENT_UPDATE:
                    result = JsonConvert.DeserializeObject<SegmentChangeNotification>(notificationData.Data);
                    break;
                default:
                    return null;
            }

            result.Channel = notificationData.Channel;

            return result;
        }

        private IncomingNotification ParseControlChannelMessage(string notificationString)
        {
            var notificationData = GetNotificationData<NotificationData>(notificationString);
            var channel = notificationData.Channel.Replace(Constans.PushOccupancyPrefix, string.Empty);

            if (notificationData.Data.Contains("controlType"))
            {
                var controlNotification = JsonConvert.DeserializeObject<ControlNotification>(notificationData.Data);
                controlNotification.Type = NotificationType.CONTROL;
                controlNotification.Channel = channel;

                return controlNotification;
            }

            var occupancyNotification = JsonConvert.DeserializeObject<OccupancyNotification>(notificationData.Data);

            if (occupancyNotification?.Metrics == null)
                return null;

            occupancyNotification.Type = NotificationType.OCCUPANCY;
            occupancyNotification.Channel = notificationData.Channel.Replace(Constans.PushOccupancyPrefix, string.Empty);

            return occupancyNotification;
        }

        private IncomingNotification ParseError(string notificationString)
        {
            var notificatinError = GetNotificationData<NotificationError>(notificationString);

            if (notificatinError.Message != null)
                throw new NotificationErrorException(notificatinError);

            return null;
        }

        private T GetNotificationData<T>(string notificationString)
        {
            var notificationArray = notificationString.Split('\n');
            var index = Array.FindIndex(notificationArray, row => row.Contains("data: "));

            return JsonConvert.DeserializeObject<T>(notificationArray[index].Replace("data: ", string.Empty));
        }
    }
}
