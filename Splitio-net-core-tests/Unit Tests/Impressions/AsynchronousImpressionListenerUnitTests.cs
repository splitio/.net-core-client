using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Impressions.Classes;
using Splitio.Domain;
using Moq;
using Splitio.Services.Impressions.Interfaces;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class AsynchronousImpressionListenerUnitTests
    {
        [TestMethod]
        public void AddListenerAndPerformLogSuccessfully()
        {
            //Arrange
            var asyncListener = new AsynchronousImpressionListener();
            var listenerMock = new Mock<IImpressionListener>();
            asyncListener.AddListener(listenerMock.Object);

            //Act
            asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            Thread.Sleep(1000);

            //Assert
            listenerMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")));
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfully()
        {
            //Arrange
            var asyncListener = new AsynchronousImpressionListener();
            var listenerMock1 = new Mock<IImpressionListener>();
            var listenerMock2 = new Mock<IImpressionListener>();
            asyncListener.AddListener(listenerMock1.Object);
            asyncListener.AddListener(listenerMock2.Object);


            //Act
            asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            Thread.Sleep(1000);

            //Assert
            listenerMock1.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")), Times.Once());
            listenerMock2.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")), Times.Once());
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfullyWhenOneListenerFails()
        {
            //Arrange
            var asyncListener = new AsynchronousImpressionListener();
            var listenerMock1 = new Mock<IImpressionListener>();
            listenerMock1.Setup(x => x.Log(It.IsAny<KeyImpression>())).Throws(new Exception());
            var listenerMock2 = new Mock<IImpressionListener>();
            asyncListener.AddListener(listenerMock1.Object);
            asyncListener.AddListener(listenerMock2.Object);


            //Act
            asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            Thread.Sleep(1000);

            //Assert
            listenerMock2.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")), Times.Once());
        }
    }
}
