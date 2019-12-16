using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Events
{
    //[TestClass]
    //public class AsynchronousEventListenerUnitTests
    //{
    //    private Mock<ISplitLogger> _logger;
    //    private AsynchronousListener<Event> _asyncListener;
    //    private Mock<IListener<Event>> _listenerMock;

    //    [TestInitialize]
    //    public void Initialize()
    //    {
    //        _logger = new Mock<ISplitLogger>();
    //        _asyncListener = new AsynchronousListener<Event>(_logger.Object);
    //        _listenerMock = new Mock<IListener<Event>>();
    //    }

    //    [TestMethod]
    //    public void AddListenerAndPerformLogSuccessfully()
    //    {
    //        //Arrange
    //        _asyncListener.AddListener(_listenerMock.Object);

    //        //Act
    //        _asyncListener.Notify(new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 });
    //        Thread.Sleep(1000);

    //        //Assert
    //        _listenerMock.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)));
    //    }

    //    [TestMethod]
    //    public void AddTwoListenersAndPerformLogSuccessfully()
    //    {
    //        //Arrange
    //        var listenerMock2 = new Mock<IListener<Event>>();
    //        _asyncListener.AddListener(_listenerMock.Object);
    //        _asyncListener.AddListener(listenerMock2.Object);

    //        //Act
    //        _asyncListener.Notify(new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 });
    //        // TODO: Adding Thread.Sleep is awful, we should create a LogAsync method that returns a task and wait for that task to finish 
    //        Thread.Sleep(2000);

    //        //Assert
    //        _listenerMock.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)), Times.Once());
    //        listenerMock2.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)), Times.Once());
    //    }

    //    [TestMethod]
    //    public void AddTwoListenersAndPerformLogSuccessfullyWhenOneListenerFails()
    //    {
    //        //Arrange
    //        var listenerMock2 = new Mock<IListener<Event>>();
    //        _listenerMock.Setup(x => x.Log(It.IsAny<Event>())).Throws(new Exception());
    //        _asyncListener.AddListener(_listenerMock.Object);
    //        _asyncListener.AddListener(listenerMock2.Object);

    //        //Act
    //        _asyncListener.Notify(new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 });
    //        Thread.Sleep(1000);

    //        //Assert
    //        listenerMock2.Verify(x => x.Log(It.Is<Event>(e => e.key == "Key1" && e.eventTypeId == "testEventType" && e.trafficTypeName == "testTrafficType" && e.timestamp == 7000 && e.value == 12.34)), Times.Once());
    //    }
    //}
}
