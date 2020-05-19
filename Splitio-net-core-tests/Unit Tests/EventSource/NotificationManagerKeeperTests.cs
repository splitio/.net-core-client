using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using System;

namespace Splitio_Tests.Unit_Tests.EventSource
{
    [TestClass]
    public class NotificationManagerKeeperTests
    {
        private readonly Mock<ISplitLogger> _log;
        private bool? _pushEnabled;
        private EventArgs _event;

        private readonly INotificationManagerKeeper _notificationManagerKeeper;

        public NotificationManagerKeeperTests()
        {
            _log = new Mock<ISplitLogger>();

            _notificationManagerKeeper = new NotificationManagerKeeper(_log.Object);
            _notificationManagerKeeper.OccupancyEvent += OnOccupancyEvent;
            _notificationManagerKeeper.PushShutdownEvent += OnPushShutdownEvent;
        }

        [TestMethod]
        public void HandleIncominEvent_ControlStreamingPaused_ShouldDispatchEvent()
        {
            // Arrange.
            _pushEnabled = null;

            var notification = new ControlNotification
            {
                 Channel = "control_pri",
                 ControlType = ControlType.STREAMING_PAUSED,
                 Type = NotificationType.CONTROL
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.IsFalse(_pushEnabled.Value);
        }

        [TestMethod]
        public void HandleIncominEvent_ControlStreamingResumed_ShouldDispatchEvent()
        {
            // Arrange.
            _pushEnabled = null;

            var notification = new ControlNotification
            {
                Channel = "control_pri",
                ControlType = ControlType.STREAMING_RESUMED,
                Type = NotificationType.CONTROL
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.IsTrue(_pushEnabled.Value);
        }

        [TestMethod]
        public void HandleIncominEvent_ControlStreamingResumed_ShouldNotDispatchEvent()
        {
            // Arrange.
            _pushEnabled = null;

            var occupancyNoti = new OccupancyNotification
            {
                Channel = "control_pri",
                Metrics = new OccupancyMetricsData { Publishers = 0 },
                Type = NotificationType.OCCUPANCY
            };

            var notification = new ControlNotification
            {
                Channel = "control_pri",
                ControlType = ControlType.STREAMING_RESUMED,
                Type = NotificationType.CONTROL
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(occupancyNoti);
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.IsFalse(_pushEnabled.Value);
        }

        [TestMethod]
        public void HandleIncominEvent_ControlStreamingDisabled_ShouldDispatchEvent()
        {
            // Arrange.
            _event = null;

            var notification = new ControlNotification
            {
                Channel = "control_pri",
                ControlType = ControlType.STREAMING_DISABLED,
                Type = NotificationType.CONTROL
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.IsNotNull(_event);
        }

        [TestMethod]
        public void HandleIncominEvent_OccupancyWithPublishers_FirstTime_ShouldNotDispatchEvent()
        {
            // Arrange.
            _pushEnabled = null;

            var notification = new OccupancyNotification
            {
                Channel = "control_pri",
                Type = NotificationType.OCCUPANCY,
                 Metrics = new OccupancyMetricsData { Publishers = 2 }
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.IsNull(_pushEnabled);
        }

        private void OnOccupancyEvent(object sender, OccupancyEventArgs e)
        {
            _pushEnabled = e.PublisherAvailable;
        }

        private void OnPushShutdownEvent(object sender, EventArgs e)
        {
            _event = e;
        }
    }
}
