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
        public string Data { get; set; }
    }

    public enum NotificationType
    {
        SPLIT_UPDATE,
        SPLIT_KILL,
        SEGMENT_UPDATE,
        CONTROL
    }

    public class IncomingNotification
    {
        public NotificationType Type { get; set; }
    }

    public class SplitChangeNotifiaction : IncomingNotification
    {
        public long ChangeNumber { get; set; }
    }

    public class SplitKillNotification : IncomingNotification
    {
        public long ChangeNumber { get; set; }
        public string DefaultTreatment { get; set; }
        public string SplitName { get; set; }
    }

    public class SegmentChangeNotification : IncomingNotification
    {
        public long ChangeNumber { get; set; }
        public string SegmentName { get; set; }
    }

    public class ControlEventData : IncomingNotification
    {
        public string ControlType { get; set; }
    }
}
