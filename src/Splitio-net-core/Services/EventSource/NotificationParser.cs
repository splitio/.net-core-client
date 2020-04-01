using Newtonsoft.Json;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationParser : INotificationParser
    {
        public EventData Parse(string text)
        {
            var notification = JsonConvert.DeserializeObject<Notification>(text);
            var dataJsonString = JsonConvert.SerializeObject(notification.Data.Data);
            var data = JsonConvert.DeserializeObject<EventData>(dataJsonString);

            switch (data?.Type)
            {
                case NotificationType.SPLIT_UPDATE:
                    return JsonConvert.DeserializeObject<SplitUpdateEventData>(dataJsonString);
                case NotificationType.SPLIT_KILL:
                    return JsonConvert.DeserializeObject<SplitKillEventData>(dataJsonString);
                case NotificationType.SEGMENT_UPDATE:
                    return JsonConvert.DeserializeObject<SegmentUpdateEventData>(dataJsonString);
                case NotificationType.CONTROL:
                    return JsonConvert.DeserializeObject<ControlEventData>(dataJsonString);
                default:
                    throw new Exception("Unexpected type received from EventSource");
            }
        }
    }
}
