using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;
using Splitio.Services.Exceptions;
using System;

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
            var text = "{\"id\":\"234234432\",\"event\":\"message\",\"data\":{\"id\":\"KXLEfWv-l4:0:0\",\"clientId\":\"3233424\",\"timestamp\":1585867724988,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_UPDATE\\\",\\\"changeNumber\\\":1585867723838}\"}}\n";

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
            var text = "{\"id\":\"23423432\",\"event\":\"message\",\"data\":{\"id\":\"vJ0EW4_EZa:0:0\",\"clientId\":\"332432324\",\"timestamp\":1585868247781,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_KILL\\\",\\\"changeNumber\\\":1585868246622,\\\"defaultTreatment\\\":\\\"off\\\",\\\"splitName\\\":\\\"test-split\\\"}\"}}\n";

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
            var text = "{\"id\":\"234432\",\"event\":\"message\",\"data\":{\"id\":\"rwlbcidVwD:0:0\",\"clientId\":\"234234234\",\"timestamp\":1585868933616,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_segments\",\"data\":\"{\\\"type\\\":\\\"SEGMENT_UPDATE\\\",\\\"changeNumber\\\":1585868933303,\\\"segmentName\\\":\\\"test-segment\\\"}\"}}\n";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(1585868933303, ((SegmentChangeNotification)result).ChangeNumber);
            Assert.AreEqual("test-segment", ((SegmentChangeNotification)result).SegmentName);
            Assert.AreEqual(NotificationType.SEGMENT_UPDATE, result.Type);
            Assert.AreEqual("xxxx_xxxx_segments", result.Channel);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
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
        }

        [TestMethod]
        [ExpectedException(typeof(NotificationErrorException))]
        public void Parse_NotificationError_ShouldReturnException()
        {
            // Arrange.
            var text = "{\n\t\"error\":{\n\t\t\"message\":\"Token expired. (See https://help.fake.io/error/40142 for help.)\",\n\t\t\"code\":40142,\n\t\t\"statusCode\":401,\n\t\t\"href\":\"https://help.ably.io/error/40142\",\n\t\t\"serverId\":\"123123\"\n\t}\n}";
            
            // Act.
            var result = _notificationParser.Parse(text);
        }

        [TestMethod]
        public void Parse_Occupancy_ControlPri_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = "{\"event\":\"message\",\"data\":{\"id\":\"BEJUTn31k2:0:0\",\"timestamp\":1588111458690,\"encoding\":\"json\",\"channel\":\"[?occupancy=metrics.publishers]control_pri\",\"data\":\"{\\\"metrics\\\":{\\\"publishers\\\":2}}\",\"name\":\"[meta]occupancy\"}}";

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
            var text = "{\"event\":\"message\",\"data\":{\"id\":\"BEJUTn31k2:0:0\",\"timestamp\":1588111458690,\"encoding\":\"json\",\"channel\":\"[?occupancy=metrics.publishers]control_sec\",\"data\":\"{\\\"metrics\\\":{\\\"publishers\\\":1}}\",\"name\":\"[meta]occupancy\"}}";

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
            var text = "{\"event\":\"message\",\"data\":{\"id\": \"Y1XJoAm7No:0:0\",\"clientId\": \"EORI49J_FSJKA2\",\"timestamp\": 1582056812285,\"encoding\": \"json\",\"channel\": \"control_pri\",\"data\": \"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_PAUSED\\\"}\"}}";

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
            var text = "{\"event\":\"message\",\"data\":{\"id\": \"Y1XJoAm7No:0:0\",\"clientId\": \"EORI49J_FSJKA2\",\"timestamp\": 1582056812285,\"encoding\": \"json\",\"channel\": \"control_pri\",\"data\": \"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_RESUMED\\\"}\"}}";

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
            var text = "{\"event\":\"message\",\"data\":{\"id\": \"Y1XJoAm7No:0:0\",\"clientId\": \"EORI49J_FSJKA2\",\"timestamp\": 1582056812285,\"encoding\": \"json\",\"channel\": \"control_pri\",\"data\": \"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_DISABLED\\\"}\"}}";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(NotificationType.CONTROL, result.Type);
            Assert.AreEqual(ControlType.STREAMING_DISABLED, ((ControlNotification)result).ControlType);
            Assert.AreEqual("control_pri", result.Channel);
        }
    }
}
