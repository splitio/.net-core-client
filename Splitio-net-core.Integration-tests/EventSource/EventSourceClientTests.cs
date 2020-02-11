using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Splitio_net_core.Integration_tests.EventSource
{
    [TestClass]
    public class EventSourceClientTests
    {
        private Queue<EventReceivedEventArgs> _eventsReceived;
        private Queue<ErrorReceivedEventArgs> _errorsReceived;

        [TestMethod]
        public void ShouldReceiveMessageEvent()
        {
            var httpClientMock = new HttpClientMock();
            httpClientMock.SSE_Channels_Response("{\"event\":\"message\",\"data\":{\"id\":\"1\",\"channel\":\"mauroc\",\"data\":\"data-test\",\"name\":\"name-test\"}}\n");

            var uri = new Uri($"http://localhost:{httpClientMock.GetPort()}");
            _eventsReceived = new Queue<EventReceivedEventArgs>();
            _errorsReceived = new Queue<ErrorReceivedEventArgs>();

            var eventSourceClient = new EventSourceClient(uri, 10000);
            eventSourceClient.EventReceived += EventReceived;
            eventSourceClient.ErrorReceived += ErrorReceived;

            Thread.Sleep(5000);

            Assert.AreEqual(0, _errorsReceived.Count);
            Assert.AreEqual(1, _eventsReceived.Count);
            var ev = _eventsReceived.Dequeue();
            Assert.AreEqual("message", ev.Event.Type);
            Assert.AreEqual("1", ev.Event.Data.Id);
            Assert.AreEqual("mauroc", ev.Event.Data.Channel);
            Assert.AreEqual("data-test", ev.Event.Data.Content);
            Assert.AreEqual("name-test", ev.Event.Data.Name);
        }

        [TestMethod]
        public void ShouldReceiveControlEvent()
        {
            var httpClientMock = new HttpClientMock();
            httpClientMock.SSE_Channels_Response("{\"event\":\"control\",\"data\":{\"id\":\"2\",\"channel\":\"mauroc\",\"data\":\"data-control-test\",\"name\":\"name-control-test\"}}\n");

            var uri = new Uri($"http://localhost:{httpClientMock.GetPort()}");
            _eventsReceived = new Queue<EventReceivedEventArgs>();
            _errorsReceived = new Queue<ErrorReceivedEventArgs>();

            var eventSourceClient = new EventSourceClient(uri, 10000);
            eventSourceClient.EventReceived += EventReceived;
            eventSourceClient.ErrorReceived += ErrorReceived;

            Thread.Sleep(5000);

            Assert.AreEqual(0, _errorsReceived.Count);
            Assert.AreEqual(1, _eventsReceived.Count);
            var ev = _eventsReceived.Dequeue();
            Assert.AreEqual("control", ev.Event.Type);
            Assert.AreEqual("2", ev.Event.Data.Id);
            Assert.AreEqual("mauroc", ev.Event.Data.Channel);
            Assert.AreEqual("data-control-test", ev.Event.Data.Content);
            Assert.AreEqual("name-control-test", ev.Event.Data.Name);
        }

        [TestMethod]
        public void ShouldReceiveInvalidEventFormat()
        {
            var httpClientMock = new HttpClientMock();
            httpClientMock.SSE_Channels_Response("{\"event\":\"control\",\"info\":{\"id\":\"2\",\"channel\":\"mauroc\",\"data\":\"data-control-test\",\"name\":\"name-control-test\"}}\n");

            var uri = new Uri($"http://localhost:{httpClientMock.GetPort()}");
            _eventsReceived = new Queue<EventReceivedEventArgs>();
            _errorsReceived = new Queue<ErrorReceivedEventArgs>();

            var eventSourceClient = new EventSourceClient(uri, 10000);
            eventSourceClient.EventReceived += EventReceived;
            eventSourceClient.ErrorReceived += ErrorReceived;

            Thread.Sleep(5000);

            Assert.AreEqual(1, _errorsReceived.Count);
            Assert.AreEqual(0, _eventsReceived.Count);
        }

        private void EventReceived(object sender, EventReceivedEventArgs e)
        {
            _eventsReceived.Enqueue(e);
        }

        private void ErrorReceived(object sender, ErrorReceivedEventArgs e)
        {
            _errorsReceived.Enqueue(e);
        }
    }
}
