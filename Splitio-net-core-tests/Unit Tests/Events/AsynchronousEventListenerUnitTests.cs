using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Shared;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Events
{
    [TestClass]
    public class AsynchronousEventListenerUnitTests
    {
        [TestMethod]
        public void AddListenerAndPerformLogSuccessfully()
        {
            //Arrange
            var logger = new Mock<ILog>();
            var asyncListener = new AsynchronousListener<Event>(logger.Object);
            var listenerMock = new Mock<IListener<Event>>();
            asyncListener.AddListener(listenerMock.Object);

            //Act
            asyncListener.Log(new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 });
            Thread.Sleep(1000);

            //Assert
            listenerMock.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)));
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfully()
        {
            //Arrange
            var logger = new Mock<ILog>();
            var asyncListener = new AsynchronousListener<Event>(logger.Object);
            var listenerMock1 = new Mock<IListener<Event>>();
            var listenerMock2 = new Mock<IListener<Event>>();
            asyncListener.AddListener(listenerMock1.Object);
            asyncListener.AddListener(listenerMock2.Object);

            //Act
            asyncListener.Log(new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 });
            // TODO: Adding Thread.Sleep is awful, we should create a LogAsync method that returns a task and wait for that task to finish 
            Thread.Sleep(2000);

            //Assert
            listenerMock1.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)), Times.Once());
            listenerMock2.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)), Times.Once());
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfullyWhenOneListenerFails()
        {
            //Arrange
            var logger = new Mock<ILog>();
            var asyncListener = new AsynchronousListener<Event>(logger.Object);
            var listenerMock1 = new Mock<IListener<Event>>();
            var listenerMock2 = new Mock<IListener<Event>>();
            listenerMock1.Setup(x => x.Log(It.IsAny<Event>())).Throws(new Exception());
            asyncListener.AddListener(listenerMock1.Object);
            asyncListener.AddListener(listenerMock2.Object);

            //Act
            asyncListener.Log(new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 });
            Thread.Sleep(1000);

            //Assert
            listenerMock2.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)), Times.Once());
        }
    }
}
