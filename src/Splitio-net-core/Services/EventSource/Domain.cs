namespace Splitio.Services.EventSource
{
    public class Notification
    {
        public string Id { get; set; }
        public string Event { get; set; }
        public NotificationData Data { get; set; }
    }

    public class NotificationData
    {
        public string Id { get; set; }
        public string Channel { get; set; }
        public object Data { get; set; }
    }

    public enum NotificationType
    {
        SPLIT_UPDATE,
        SPLIT_KILL,
        SEGMENT_UPDATE,
        CONTROL
    }

    public class EventData
    {
        public NotificationType Type { get; set; }
    }

    public class SplitUpdateEventData : EventData
    {
        public long ChangeNumber { get; set; }
    }

    public class SplitKillEventData : EventData
    {
        public long ChangeNumber { get; set; }
        public string DefaultTreatment { get; set; }
        public string SplitName { get; set; }
    }

    public class SegmentUpdateEventData : EventData
    {
        public long ChangeNumber { get; set; }
        public string SegmentName { get; set; }
    }

    public class ControlEventData : EventData
    {
        public string ControlType { get; set; }
    }
}
