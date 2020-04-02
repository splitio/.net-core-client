using Newtonsoft.Json;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationParser : INotificationParser
    {
        public IncomingNotification Parse(string text)
        {
            var notification = JsonConvert.DeserializeObject<Notification>(text);
            var data = JsonConvert.DeserializeObject<IncomingNotification>(notification?.Data?.Data ?? string.Empty);

            switch (data?.Type)
            {
                case NotificationType.SPLIT_UPDATE:
                    return JsonConvert.DeserializeObject<SplitChangeNotifiaction>(notification.Data.Data);
                case NotificationType.SPLIT_KILL:
                    return JsonConvert.DeserializeObject<SplitKillNotification>(notification.Data.Data);
                case NotificationType.SEGMENT_UPDATE:
                    return JsonConvert.DeserializeObject<SegmentChangeNotification>(notification.Data.Data);
                case NotificationType.CONTROL:
                    return JsonConvert.DeserializeObject<ControlEventData>(notification.Data.Data);
                default:
                    throw new Exception("Unexpected type received from EventSource");
            }
        }
    }
}
