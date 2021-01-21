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
        private SSEActionsEventArgs _event;

        private readonly INotificationManagerKeeper _notificationManagerKeeper;

        public NotificationManagerKeeperTests()
        {
            _log = new Mock<ISplitLogger>();

            _notificationManagerKeeper = new NotificationManagerKeeper(_log.Object);
            _notificationManagerKeeper.ActionEvent += OnActionEvent;
        }

        [TestMethod]
        public void HandleIncominEvent_ControlStreamingPaused_ShouldDispatchEvent()
        {
            // Arrange.
            _event = null;

            var notification = new ControlNotification
            {
                 Channel = "control_pri",
                 ControlType = ControlType.STREAMING_PAUSED,
                 Type = NotificationType.CONTROL
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_DOWN, _event.Action);
        }

        [TestMethod]
        public void HandleIncominEvent_ControlStreamingResumed_ShouldDispatchEvent()
        {
            // Arrange.
            _event = null;

            var notification = new ControlNotification
            {
                Channel = "control_pri",
                ControlType = ControlType.STREAMING_RESUMED,
                Type = NotificationType.CONTROL
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_READY, _event.Action);
        }

        [TestMethod]
        public void HandleIncominEvent_ControlStreamingResumed_ShouldNotDispatchEvent()
        {
            // Arrange.
            _event = null;

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

            // Act & Assert.
            _notificationManagerKeeper.HandleIncomingEvent(occupancyNoti);
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_DOWN, _event.Action);

            _event = null;
            _notificationManagerKeeper.HandleIncomingEvent(notification);
            Assert.IsNull(_event);
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
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_OFF, _event.Action);
        }

        [TestMethod]
        public void HandleIncominEvent_OccupancyWithPublishers_FirstTime_ShouldNotDispatchEvent()
        {
            // Arrange.
            _event = null;

            var notification = new OccupancyNotification
            {
                Channel = "control_pri",
                Type = NotificationType.OCCUPANCY,
                 Metrics = new OccupancyMetricsData { Publishers = 2 }
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notification);

            // Assert.
            Assert.IsNull(_event);
        }
        
        [TestMethod]
        public void HandleIncominEvent_OccupancyWithPublishers_ManyEvents_MultiReg()
        {
            // Arrange.
            _event = null;

            var notificationPri = new OccupancyNotification
            {
                Channel = "control_pri",
                Type = NotificationType.OCCUPANCY,
                Metrics = new OccupancyMetricsData { Publishers = 2 }
            };            

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationPri);

            // Assert.
            Assert.IsNull(_event);

            // Event control_pri with 0 publishers - should return false
            // Arrange.
            notificationPri.Metrics.Publishers = 0;

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationPri);

            // Assert.
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_DOWN, _event.Action);

            // Event control_sec with 2 publishers - should return true
            // Arrange.
            var notificationSec = new OccupancyNotification
            {
                Channel = "control_sec",
                Type = NotificationType.OCCUPANCY,
                Metrics = new OccupancyMetricsData { Publishers = 2 }
            };

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationSec);

            // Assert.
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_READY, _event.Action);

            // Event control_pri with 2 publishers - should return null
            // Arrange.
            _event = null;
            notificationPri.Metrics.Publishers = 2;

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationPri);

            // Assert.
            Assert.IsNull(_event);

            // Event control_pri with 0 publishers - should return null
            // Arrange.
            notificationPri.Metrics.Publishers = 0;

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationPri);

            // Assert.
            Assert.IsNull(_event);

            // Event control_sec with 0 publishers - should return false
            // Arrange.
            notificationSec.Metrics.Publishers = 0;

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationSec);

            // Assert.
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_DOWN, _event.Action);

            // Event control_sec with 0 publishers - should return null
            // Arrange.
            _event = null;
            notificationSec.Metrics.Publishers = 0;

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationSec);

            // Assert.
            Assert.IsNull(_event);

            // Event control_sec with 1 publishers - should return true
            // Arrange.
            notificationSec.Metrics.Publishers = 1;

            // Act.
            _notificationManagerKeeper.HandleIncomingEvent(notificationSec);

            // Assert.
            Assert.AreEqual(SSEClientActions.SUBSYSTEM_READY, _event.Action);
        }
        
        private void OnActionEvent(object sender, SSEActionsEventArgs e)
        {
            _event = e;
        }
    }
}
