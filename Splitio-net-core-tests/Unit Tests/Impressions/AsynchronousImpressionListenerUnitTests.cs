using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class AsynchronousImpressionListenerUnitTests
    {
        private Mock<ILog> _logger;
        private AsynchronousListener<KeyImpression> _asyncListener;
        private Mock<IListener<KeyImpression>> _listenerMock;

        [TestInitialize]
        public void Initialize()
        {
            _logger = new Mock<ILog>();
            _asyncListener = new AsynchronousListener<KeyImpression>(_logger.Object);
            _listenerMock = new Mock<IListener<KeyImpression>>();
        }

        [TestMethod]
        public void AddListenerAndPerformLogSuccessfully()
        {
            //Arrange
            _asyncListener.AddListener(_listenerMock.Object);

            //Act
            _asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            Thread.Sleep(1000);

            //Assert
            _listenerMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")));
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfully()
        {
            //Arrange
            var listenerMock2 = new Mock<IListener<KeyImpression>>();
            _asyncListener.AddListener(_listenerMock.Object);
            _asyncListener.AddListener(listenerMock2.Object);


            //Act
            _asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            // TODO: Adding Thread.Sleep is awful, we should create a LogAsync method that returns a task and wait for that task to finish 
            Thread.Sleep(2000);

            //Assert
            _listenerMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")), Times.Once());
            listenerMock2.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")), Times.Once());
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfullyWhenOneListenerFails()
        {
            //Arrange
            _listenerMock.Setup(x => x.Log(It.IsAny<KeyImpression>())).Throws(new Exception());
            var listenerMock2 = new Mock<IListener<KeyImpression>>();
            _asyncListener.AddListener(_listenerMock.Object);
            _asyncListener.AddListener(listenerMock2.Object);


            //Act
            _asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            Thread.Sleep(1000);

            //Assert
            listenerMock2.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")), Times.Once());
        }
    }
}
