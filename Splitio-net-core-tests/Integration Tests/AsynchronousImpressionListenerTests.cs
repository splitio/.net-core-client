using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio_Tests.Integration_Tests
{
    //[TestClass]
    //public class AsynchronousImpressionListenerTests
    //{
    //    public class TestListener : IListener<KeyImpression>
    //    {
    //        public List<KeyImpression> list = new List<KeyImpression>();

    //        public virtual void Log(KeyImpression impression)
    //        {
    //            list.Add(impression);
    //        }
    //    }

    //    public class TestListener2 : TestListener
    //    {
    //        public override void Log(KeyImpression impression)
    //        {
    //            //To simulate retry attemps before success
    //            for (int i = 1; i <= 100; i++)
    //                Thread.Sleep(100);

    //            list.Add(impression);
    //        }
    //    }

    //    [TestMethod]
    //    public void AddTwoListenersAndPerformLogSuccessfully()
    //    {
    //        //Arrange
    //        var logger = new Mock<ISplitLogger>();
    //        var asyncListener = new AsynchronousListener<KeyImpression>(logger.Object);
    //        var listener1 = new TestListener();
    //        var listenerMock2 = new Mock<IListener<KeyImpression>>();
    //        asyncListener.AddListener(listener1);
    //        asyncListener.AddListener(listenerMock2.Object);

    //        var impression = new KeyImpression
    //        {
    //            feature = "test",
    //            changeNumber = 100,
    //            keyName = "date",
    //            label = "testdate",
    //            time = 10000000,
    //            treatment = "on",
    //            bucketingKey = "any"
    //        };

    //        //Act
    //        asyncListener.Notify(impression);

    //        //Assert
    //        SleepThread(3, listener1);
    //        Assert.AreEqual(listener1.list.Count, 1);
    //        listenerMock2.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == impression.keyName &&
    //                                                                  p.feature == impression.feature &&
    //                                                                  p.treatment == impression.treatment &&
    //                                                                  p.time == impression.time &&
    //                                                                  p.changeNumber == impression.changeNumber &&
    //                                                                  p.label == impression.label &&
    //                                                                  p.bucketingKey == impression.bucketingKey)), Times.Once());
    //    }

    //    [TestMethod]
    //    public void AddTwoListenersAndPerformLogSuccessfullyWithOneLongRunningTask()
    //    {
    //        //Arrange
    //        var logger = new Mock<ISplitLogger>();
    //        var asyncListener = new AsynchronousListener<KeyImpression>(logger.Object);
    //        var listener1 = new TestListener2();
    //        var listener2 = new TestListener();
    //        asyncListener.AddListener(listener1);
    //        asyncListener.AddListener(listener2);
            
    //        //Act
    //        asyncListener.Notify(new KeyImpression { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000, treatment = "on", bucketingKey = "any" });

    //        //Assert
    //        SleepThread(3, listener2);
    //        Assert.AreEqual(listener2.list.Count, 1);

    //        SleepThread(20, listener1);
    //        Assert.AreEqual(listener1.list.Count, 1);
    //    }

    //    private void SleepThread(int attempts, TestListener listener)
    //    {
    //        for (int i = 1; i <= attempts; i++)
    //        {
    //            if (!listener.list.Any())
    //            {
    //                Thread.Sleep(i * 500);
    //            }
    //        }
    //    }
    //}
}
