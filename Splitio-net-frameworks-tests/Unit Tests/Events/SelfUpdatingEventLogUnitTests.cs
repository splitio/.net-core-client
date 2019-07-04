using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Events.Classes;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Shared.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio_net_frameworks_tests.Unit_Tests.Events
{
    [TestClass]
    public class SelfUpdatingEventLogUnitTests
    {
        private BlockingQueue<WrappedEvent> _queue;
        private InMemorySimpleCache<WrappedEvent> _eventsCache;
        private Mock<IEventSdkApiClient> _apiClientMock;
        private SelfUpdatingEventLog _eventLog;

        [TestInitialize]
        public void Initialize()
        {
            _queue = new BlockingQueue<WrappedEvent>(10);
            _eventsCache = new InMemorySimpleCache<WrappedEvent>(_queue);
            _apiClientMock = new Mock<IEventSdkApiClient>();
            _eventLog = new SelfUpdatingEventLog(_apiClientMock.Object, 1, 1, _eventsCache, 10);
        }

        [TestMethod]
        public void LogSuccessfullyWithNoValue()
        {
            //Act
            var eventToLog = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000 };

            _eventLog.Log(new WrappedEvent
            {
                Event = eventToLog,
                Size = 1024
            });

            //Assert
            WrappedEvent element = null;
            while (element == null)
            {
                element = _queue.Dequeue();
            }
            Assert.IsNotNull(element);
            Assert.AreEqual("Key1", element.Event.key);
            Assert.AreEqual("testEventType", element.Event.eventTypeId);
            Assert.AreEqual("testTrafficType", element.Event.trafficTypeName);
            Assert.AreEqual(7000, element.Event.timestamp);
            Assert.IsNull(element.Event.value);
        }

        [TestMethod]
        public void LogSuccessfullyWithValue()
        {
            //Act
            var eventToLog = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 };

            _eventLog.Log(new WrappedEvent
            {
                Event = eventToLog,
                Size = 1024
            });

            //Assert
            WrappedEvent element = null;
            while (element == null)
            {
                element = _queue.Dequeue();
            }
            Assert.IsNotNull(element);
            Assert.AreEqual("Key1", element.Event.key);
            Assert.AreEqual("testEventType", element.Event.eventTypeId);
            Assert.AreEqual("testTrafficType", element.Event.trafficTypeName);
            Assert.AreEqual(7000, element.Event.timestamp);
            Assert.AreEqual(12.34, element.Event.value);
        }

        [TestMethod]
        public void LogSuccessfullyAndSendEventsWithNoValue()
        {
            //Act
            _eventLog.Start();
            var eventToLog = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000 };

            _eventLog.Log(new WrappedEvent
            {
                Event = eventToLog,
                Size = 1024
            });

            //Assert
            Thread.Sleep(2000);
            _apiClientMock.Verify(x => x.SendBulkEvents(It.Is<List<Event>>(list => list.Count == 1 && list[0].value == null)));
        }

        [TestMethod]
        public void LogSuccessfullyAndSendEventsWithValue()
        {
            //Act
            _eventLog.Start();
            var eventToLog = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 };

            _eventLog.Log(new WrappedEvent
            {
                Event = eventToLog,
                Size = 1024
            });

            //Assert
            Thread.Sleep(2000);
            _apiClientMock.Verify(x => x.SendBulkEvents(It.Is<List<Event>>(list => list.Count == 1 && list[0].value != null)));
        }

        [TestMethod]
        public void LogEvent_WhenEventSizeIsbiggerThatWeSupport_ShouldsSendBulkEventsOnce()
        {
            // Arrange.
            var eventCountExpected = 3;

            var eventToLog1 = new Event { key = "Key1", eventTypeId = "testEventType1", trafficTypeName = "testTrafficType1", timestamp = 7000, value = 12.34 };
            var eventToLog2 = new Event { key = "Key2", eventTypeId = "testEventType2", trafficTypeName = "testTrafficType2", timestamp = 6000 };
            var eventToLog3 = new Event { key = "Key3", eventTypeId = "testEventType3", trafficTypeName = "testTrafficType3", timestamp = 5000, value = 15.56 };
            var eventToLog4 = new Event { key = "Key4", eventTypeId = "testEventType4", trafficTypeName = "testTrafficType4", timestamp = 8000 };

            // Act.
            _eventLog.Log(new WrappedEvent { Event = eventToLog1, Size = 1747627 });
            _eventLog.Log(new WrappedEvent { Event = eventToLog2, Size = 1747627 });
            _eventLog.Log(new WrappedEvent { Event = eventToLog3, Size = 1747627 });
            _eventLog.Log(new WrappedEvent { Event = eventToLog4, Size = 1747627 });

            // Assert.
            _apiClientMock.Verify(x => x.SendBulkEvents(It.Is<List<Event>>(list => list.Count == eventCountExpected
                                                                                && list.Any(l => l.key.Equals(eventToLog1.key)
                                                                                              && l.eventTypeId.Equals(eventToLog1.eventTypeId)
                                                                                              && l.trafficTypeName.Equals(eventToLog1.trafficTypeName)
                                                                                              && l.value == eventToLog1.value)
                                                                                && list.Any(l => l.key.Equals(eventToLog2.key)
                                                                                              && l.eventTypeId.Equals(eventToLog2.eventTypeId)
                                                                                              && l.trafficTypeName.Equals(eventToLog2.trafficTypeName)
                                                                                              && l.value == null)
                                                                                && list.Any(l => l.key.Equals(eventToLog3.key)
                                                                                              && l.eventTypeId.Equals(eventToLog3.eventTypeId)
                                                                                              && l.trafficTypeName.Equals(eventToLog3.trafficTypeName)
                                                                                              && l.value == eventToLog3.value))), Times.Exactly(1));

            _apiClientMock.Verify(x => x.SendBulkEvents(It.IsAny<List<Event>>()), Times.Exactly(1));
        }

        [TestMethod]
        public void LogEvent_WhenEventSizeIsbiggerThatWeSupport_ShouldsSendBulkEventsTwice()
        {
            // Arrange.
            var eventToLog1 = new Event { key = "Key1", eventTypeId = "testEventType1", trafficTypeName = "testTrafficType1", timestamp = 7000, value = 12.34 };
            var eventToLog2 = new Event { key = "Key2", eventTypeId = "testEventType2", trafficTypeName = "testTrafficType2", timestamp = 6000 };
            var eventToLog3 = new Event { key = "Key3", eventTypeId = "testEventType3", trafficTypeName = "testTrafficType3", timestamp = 5000, value = 15.56 };
            var eventToLog4 = new Event { key = "Key4", eventTypeId = "testEventType4", trafficTypeName = "testTrafficType4", timestamp = 8000 };

            // Act.
            _eventLog.Log(new WrappedEvent { Event = eventToLog1, Size = 1747627 });
            _eventLog.Log(new WrappedEvent { Event = eventToLog2, Size = 1747627 });
            _eventLog.Log(new WrappedEvent { Event = eventToLog3, Size = 1747627 });
            _eventLog.Log(new WrappedEvent { Event = eventToLog4, Size = 1747627 });
            _eventLog.Start();

            // Assert.
            _apiClientMock.Verify(x => x.SendBulkEvents(It.Is<List<Event>>(list => list.Count == 3
                                                                                && list.Any(l => l.key.Equals(eventToLog1.key)
                                                                                              && l.eventTypeId.Equals(eventToLog1.eventTypeId)
                                                                                              && l.trafficTypeName.Equals(eventToLog1.trafficTypeName)
                                                                                              && l.value == eventToLog1.value)
                                                                                && list.Any(l => l.key.Equals(eventToLog2.key)
                                                                                              && l.eventTypeId.Equals(eventToLog2.eventTypeId)
                                                                                              && l.trafficTypeName.Equals(eventToLog2.trafficTypeName)
                                                                                              && l.value == null)
                                                                                && list.Any(l => l.key.Equals(eventToLog3.key)
                                                                                              && l.eventTypeId.Equals(eventToLog3.eventTypeId)
                                                                                              && l.trafficTypeName.Equals(eventToLog3.trafficTypeName)
                                                                                              && l.value == eventToLog3.value))), Times.Exactly(1));

            Thread.Sleep(2000);
            _apiClientMock.Verify(x => x.SendBulkEvents(It.Is<List<Event>>(list => list.Count == 1
                                                                                && list.Any(l => l.key.Equals(eventToLog4.key)
                                                                                              && l.eventTypeId.Equals(eventToLog4.eventTypeId)
                                                                                              && l.trafficTypeName.Equals(eventToLog4.trafficTypeName)
                                                                                              && l.value == null))), Times.Exactly(1));

            _apiClientMock.Verify(x => x.SendBulkEvents(It.IsAny<List<Event>>()), Times.Exactly(2));
        }
    }
}
