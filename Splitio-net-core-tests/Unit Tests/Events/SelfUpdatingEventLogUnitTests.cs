using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Events.Classes;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Shared.Classes;
using System.Collections.Generic;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Events
{
    [TestClass]
    public class SelfUpdatingEventLogUnitTests
    {
        private BlockingQueue<Event> _queue;
        private InMemorySimpleCache<Event> _eventsCache;
        private Mock<IEventSdkApiClient> _apiClientMock;
        private SelfUpdatingEventLog _eventLog;

        [TestInitialize]
        public void Initialize()
        {
            _queue = new BlockingQueue<Event>(10);
            _eventsCache = new InMemorySimpleCache<Event>(_queue);
            _apiClientMock = new Mock<IEventSdkApiClient>();
            _eventLog = new SelfUpdatingEventLog(_apiClientMock.Object, 1, 1, _eventsCache, 10);
        }

        [TestMethod]
        public void LogSuccessfullyWithNoValue()
        {
            //Act
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000 };
            _eventLog.Log(e);

            //Assert
            Event element = null;
            while (element == null)
            {
                element = _queue.Dequeue();
            }
            Assert.IsNotNull(element);
            Assert.AreEqual("Key1", element.key);
            Assert.AreEqual("testEventType", element.eventTypeId);
            Assert.AreEqual("testTrafficType", element.trafficTypeName);
            Assert.AreEqual(7000, element.timestamp);
            Assert.IsNull(element.value);
        }

        [TestMethod]
        public void LogSuccessfullyWithValue()
        {
            //Act
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 };
            _eventLog.Log(e);

            //Assert
            Event element = null;
            while (element == null)
            {
                element = _queue.Dequeue();
            }
            Assert.IsNotNull(element);
            Assert.AreEqual("Key1", element.key);
            Assert.AreEqual("testEventType", element.eventTypeId);
            Assert.AreEqual("testTrafficType", element.trafficTypeName);
            Assert.AreEqual(7000, element.timestamp);
            Assert.AreEqual(12.34, element.value);
        }

        [TestMethod]
        public void LogSuccessfullyAndSendEventsWithNoValue()
        {
            //Act
            _eventLog.Start();
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000 };
            _eventLog.Log(e);

            //Assert
            Thread.Sleep(2000);
            _apiClientMock.Verify(x => x.SendBulkEvents(It.Is<List<Event>>(list => list.Count == 1 && list[0].value == null)));
        }

        [TestMethod]
        public void LogSuccessfullyAndSendEventsWithValue()
        {
            //Act
            _eventLog.Start();
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 };
            _eventLog.Log(e);

            //Assert
            Thread.Sleep(2000);
            _apiClientMock.Verify(x => x.SendBulkEvents(It.Is<List<Event>>(list => list.Count == 1 && list[0].value != null)));
        }
    }
}
