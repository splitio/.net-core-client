using Splitio.Services.EventSource;
using System;

namespace Splitio.Services.Exceptions
{
    public class NotificationErrorException : Exception
    {
        public NotificationError Notification { get; set; }

        public NotificationErrorException(NotificationError notification)
            : base(notification.Error.Message)
        {
            Notification = notification;
        }
    }
}
