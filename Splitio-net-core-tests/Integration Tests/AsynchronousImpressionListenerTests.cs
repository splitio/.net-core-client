using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Impressions.Classes;
using Splitio.Domain;
using Moq;
using Splitio.Services.Impressions.Interfaces;
using System.Threading;
using System.Collections.Generic;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class AsynchronousImpressionListenerTests
    {
        public class TestListener: IImpressionListener
        {
            public List<KeyImpression> list = new List<KeyImpression>();

            public void Log(KeyImpression impression)
            {
                list.Add(impression);
            }
        }

        public class TestListener2 : IImpressionListener
        {
            public List<KeyImpression> list = new List<KeyImpression>();

            public void Log(KeyImpression impression)
            {
                //To simulate retry attemps before success
                for (int i = 1; i <= 100; i++)
                    Thread.Sleep(100);

                list.Add(impression);
            }
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfully()
        {
            //Arrange
            var asyncListener = new AsynchronousImpressionListener();
            var listener1 = new TestListener();
            var listenerMock2 = new Mock<IImpressionListener>();
            asyncListener.AddListener(listener1);
            asyncListener.AddListener(listenerMock2.Object);


            //Act
            asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            Thread.Sleep(1000);

            //Assert
            Assert.AreEqual(listener1.list.Count, 1);
            listenerMock2.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "date" && p.feature == "test" && p.treatment == "on" && p.time == 10000000 && p.changeNumber == 100 && p.label == "testdate" && p.bucketingKey == "any")), Times.Once());
        }

        [TestMethod]
        public void AddTwoListenersAndPerformLogSuccessfullyWithOneLongRunningTask()
        {
            //Arrange
            var asyncListener = new AsynchronousImpressionListener();
            var listener1 = new TestListener2();
            var listener2 = new TestListener();
            asyncListener.AddListener(listener1);
            asyncListener.AddListener(listener2);


            //Act
            asyncListener.Log(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });
            Thread.Sleep(1000);

            //Assert
            Assert.AreEqual(listener2.list.Count, 1);
            Thread.Sleep(10000);
            Assert.AreEqual(listener1.list.Count, 1);
        }
      
    }
}
