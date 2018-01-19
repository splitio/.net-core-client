using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Events.Classes;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Shared;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Events
{
    [TestClass]
    public class SelfUpdatingEventLogUnitTests
    {
        [TestMethod]
        public void LogSuccessfullyWithNoValue()
        {
            //Arrange
            var queue = new BlockingQueue<Event>(10);
            var eventsCache = new InMemorySimpleCache<Event>(queue);
            var eventLog = new SelfUpdatingEventLog(null, 1, eventsCache, 10);

            //Act
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000 };
            eventLog.Log(e);

            //Assert
            Event element = null;
            while (element == null)
            {
                element = queue.Dequeue();
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
            //Arrange
            var queue = new BlockingQueue<Event>(10);
            var eventsCache = new InMemorySimpleCache<Event>(queue);
            var eventLog = new SelfUpdatingEventLog(null, 1, eventsCache, 10);

            //Act
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 };
            eventLog.Log(e);

            //Assert
            Event element = null;
            while (element == null)
            {
                element = queue.Dequeue();
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
            //Arrange
            var apiClientMock = new Mock<IEventSdkApiClient>();
            var queue = new BlockingQueue<Event>(10);
            var eventsCache = new InMemorySimpleCache<Event>(queue);
            var eventLog = new SelfUpdatingEventLog(apiClientMock.Object, 1, eventsCache, 10);

            //Act
            eventLog.Start();
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000 };
            eventLog.Log(e);

            //Assert
            Thread.Sleep(2000);
            apiClientMock.Verify(x => x.SendBulkEvents(It.Is<string>(s => s.IndexOf("value") < 0)));
        }

        [TestMethod]
        public void LogSuccessfullyAndSendEventsWithValue()
        {
            //Arrange
            var apiClientMock = new Mock<IEventSdkApiClient>();
            var queue = new BlockingQueue<Event>(10);
            var eventsCache = new InMemorySimpleCache<Event>(queue);
            var eventLog = new SelfUpdatingEventLog(apiClientMock.Object, 1, eventsCache, 10);

            //Act
            eventLog.Start();
            var e = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 };
            eventLog.Log(e);

            //Assert
            Thread.Sleep(2000);
            apiClientMock.Verify(x => x.SendBulkEvents(It.Is<string>(s => s.IndexOf("value") > 0)));
        }
    }
}
