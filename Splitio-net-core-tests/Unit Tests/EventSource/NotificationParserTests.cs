using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;
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
            var text = @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'data': {
                                'type': 'SPLIT_UPDATE', 
                                'changeNumber': 1254564
                            },
                            'name':'name-test'
                        }
                       }";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(1254564, ((SplitUpdateEventData)result).ChangeNumber);
            Assert.AreEqual(NotificationType.SPLIT_UPDATE, result.Type);
        }

        [TestMethod]
        public void Parse_SlitKill_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'data': {
                                'type': 'SPLIT_KILL', 
                                'changeNumber': 1254564,
                                'defaultTreatment':'off',
                                'splitName': 'test-split'
                            },
                            'name':'name-test'
                        }
                       }";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(1254564, ((SplitKillEventData)result).ChangeNumber);
            Assert.AreEqual("off", ((SplitKillEventData)result).DefaultTreatment);
            Assert.AreEqual("test-split", ((SplitKillEventData)result).SplitName);
            Assert.AreEqual(NotificationType.SPLIT_KILL, result.Type);
        }

        [TestMethod]
        public void Parse_SegmentUpdate_ShouldReturnParsedEvent()
        {
            // Arrange.
            var text = @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'data': {
                                'type': 'SEGMENT_UPDATE', 
                                'changeNumber': 1254564,
                                'segmentName': 'test-segment'
                            },
                            'name':'name-test'
                         }
                        }";

            // Act.
            var result = _notificationParser.Parse(text);

            // Assert.
            Assert.AreEqual(1254564, ((SegmentUpdateEventData)result).ChangeNumber);
            Assert.AreEqual("test-segment", ((SegmentUpdateEventData)result).SegmentName);
            Assert.AreEqual(NotificationType.SEGMENT_UPDATE, result.Type);
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
    }
}
