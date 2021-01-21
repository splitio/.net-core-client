using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;

namespace Splitio_Tests.Unit_Tests.EventSource
{
    [TestClass]
    public class NotificationParserTests
    {
        private readonly INotificationParser _notificationParser;

        public NotificationParserTests()
        {
            _notificationParser = new NotificationParser();
        }

        [TestMethod]
        public void Parse_SlitUpdate_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "id: e7dsDAgMQAkPkG@1588254699243-0\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_UPDATE\\\",\\\"changeNumber\\\":1585867723838}\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(1585867723838, ((SplitChangeNotifiaction)result).ChangeNumber);
            Assert.AreEqual(NotificationType.SPLIT_UPDATE, result.Type);
            Assert.AreEqual("xxxx_xxxx_splits", result.Channel);
        }

        [TestMethod]
        public void Parse_SlitKill_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "id: e7dsDAgMQAkPkG@1588254699243-0\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_KILL\\\",\\\"changeNumber\\\":1585868246622,\\\"defaultTreatment\\\":\\\"off\\\",\\\"splitName\\\":\\\"test-split\\\"}\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(1585868246622, ((SplitKillNotification)result).ChangeNumber);
            Assert.AreEqual("off", ((SplitKillNotification)result).DefaultTreatment);
            Assert.AreEqual("test-split", ((SplitKillNotification)result).SplitName);
            Assert.AreEqual(NotificationType.SPLIT_KILL, result.Type);
            Assert.AreEqual("xxxx_xxxx_splits", result.Channel);
        }

        [TestMethod]
        public void Parse_SegmentUpdate_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "id: e7dsDAgMQAkPkG@1588254699243-0\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_segments\",\"data\":\"{\\\"type\\\":\\\"SEGMENT_UPDATE\\\",\\\"changeNumber\\\":1588254698186,\\\"segmentName\\\":\\\"test-segment\\\"}\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(1588254698186, ((SegmentChangeNotification)result).ChangeNumber);
            Assert.AreEqual("test-segment", ((SegmentChangeNotification)result).SegmentName);
            Assert.AreEqual(NotificationType.SEGMENT_UPDATE, result.Type);
            Assert.AreEqual("xxxx_xxxx_segments", result.Channel);
        }

        [TestMethod]
        public void Parse_IncorrectFormat_ShouldReturnException()
        {
            // Arrange.
            var text = @"{ 'event': 'message', 
                           'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'content': {
                                'type': 'CONTROL', 
                                'controlType': 'test-control-type'
                            },
                            'name':'name-test'
                         }
                        }";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Parse_NotificationError_ShouldReturnException()
        {
            // Arrange.
            var text = "event: error\ndata: {\"message\":\"Token expired\",\"code\":40142,\"statusCode\":401,\"href\":\"https://help.ably.io/error/40142\"}\n\n";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(NotificationType.ERROR, result.Type);
            Assert.AreEqual(40142, ((NotificationError)result).Code);
            Assert.AreEqual(401, ((NotificationError)result).StatusCode);
        }

        [TestMethod]
        public void Parse_Occupancy_ControlPri_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "event: message\ndata: {\"id\":\"NhK8u2JPan:0:0\",\"timestamp\":1588254668328,\"encoding\":\"json\",\"channel\":\"[?occupancy=metrics.publishers]control_pri\",\"data\":\"{\\\"metrics\\\":{\\\"publishers\\\":2}}\",\"name\":\"[meta]occupancy\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(NotificationType.OCCUPANCY, result.Type);
            Assert.AreEqual(2, ((OccupancyNotification)result).Metrics.Publishers);
            Assert.AreEqual("control_pri", result.Channel);
        }

        [TestMethod]
        public void Parse_Occupancy_ControlSec_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "event: message\ndata: {\"id\":\"NhK8u2JPan:0:0\",\"timestamp\":1588254668328,\"encoding\":\"json\",\"channel\":\"[?occupancy=metrics.publishers]control_sec\",\"data\":\"{\\\"metrics\\\":{\\\"publishers\\\":1}}\",\"name\":\"[meta]occupancy\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(NotificationType.OCCUPANCY, result.Type);
            Assert.AreEqual(1, ((OccupancyNotification)result).Metrics.Publishers);
            Assert.AreEqual("control_sec", result.Channel);
        }

        [TestMethod]
        public void Parse_Control_StreamingPaused_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "event: message\ndata: {\"id\":\"2222\",\"clientId\":\"3333\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"[?occupancy=metrics.publishers]control_pri\",\"data\":\"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_PAUSED\\\"}\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(NotificationType.CONTROL, result.Type);
            Assert.AreEqual(ControlType.STREAMING_PAUSED, ((ControlNotification)result).ControlType);
            Assert.AreEqual("control_pri", result.Channel);
        }

        [TestMethod]
        public void Parse_Control_StreamingResumed_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "event: message\ndata: {\"id\":\"2222\",\"clientId\":\"3333\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"[?occupancy=metrics.publishers]control_pri\",\"data\":\"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_RESUMED\\\"}\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(NotificationType.CONTROL, result.Type);
            Assert.AreEqual(ControlType.STREAMING_RESUMED, ((ControlNotification)result).ControlType);
            Assert.AreEqual("control_pri", result.Channel);
        }

        [TestMethod]
        public void Parse_Control_StreamingDisabledShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "event: message\ndata: {\"id\":\"2222\",\"clientId\":\"3333\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"[?occupancy=metrics.publishers]control_pri\",\"data\":\"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_DISABLED\\\"}\"}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(NotificationType.CONTROL, result.Type);
            Assert.AreEqual(ControlType.STREAMING_DISABLED, ((ControlNotification)result).ControlType);
            Assert.AreEqual("control_pri", result.Channel);
        }
    }
}
