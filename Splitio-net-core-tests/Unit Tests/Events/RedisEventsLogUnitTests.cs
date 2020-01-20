using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Events.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Events
{
    [TestClass]
    public class RedisEventsLogUnitTests
    {
        private readonly Mock<ISimpleCache<WrappedEvent>> _eventsCacheMock;
        private readonly RedisEvenstLog _redisEventsLog;

        public RedisEventsLogUnitTests()
        {
            _eventsCacheMock = new Mock<ISimpleCache<WrappedEvent>>();

            _redisEventsLog = new RedisEvenstLog(_eventsCacheMock.Object);
        }

        [TestMethod]
        public void LogSuccessfully()
        {
            //Arrange
            var eventToLog = new Event { key = "Key1", eventTypeId = "testEventType", trafficTypeName = "testTrafficType", timestamp = 7000, value = 12.34 };

            var wrappedEvent = new WrappedEvent
            {
                Event = eventToLog,
                Size = 1024
            };

            //Act
            _redisEventsLog.Log(wrappedEvent);

            //Assert
            _eventsCacheMock.Verify(mock => mock.AddItems(It.IsAny<IList<WrappedEvent>>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Start_ReturnsException()
        {
            //Act
            _redisEventsLog.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Stop_ReturnsException()
        {
            //Act
            _redisEventsLog.Stop();
        }
    }
}
