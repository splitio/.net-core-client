using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Splitio_net_core.Integration_tests.EventSource
{
    [TestClass]
    public class EventSourceClientTests
    {
        private BlockingCollection<EventReceivedEventArgs> _eventsReceived;
        private BlockingCollection<EventArgs> _connectedEvent;
        private BlockingCollection<EventArgs> _disconnectEvent;

        [TestMethod]
        public void EventSourceClient_SplitUpdateEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification  = "id: 234234432\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_UPDATE\\\",\\\"changeNumber\\\":1585867723838}\"}";
                httpClientMock.SSE_Channels_Response(notification);

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _eventsReceived.TryTake(out EventReceivedEventArgs ev, -1);
                Assert.IsTrue(eventSourceClient.IsConnected());
                Assert.AreEqual(NotificationType.SPLIT_UPDATE, ev.Event.Type);
                Assert.AreEqual(1585867723838, ((SplitChangeNotifiaction)ev.Event).ChangeNumber);
                Assert.AreEqual(1, _connectedEvent.Count);

                eventSourceClient.Disconnect();
            }
        }

        [TestMethod]
        public void EventSourceClient_SplitKillEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "id: 234234432\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_KILL\\\",\\\"changeNumber\\\":1585868246622,\\\"defaultTreatment\\\":\\\"off\\\",\\\"splitName\\\":\\\"test-split\\\"}\"}";
                httpClientMock.SSE_Channels_Response(notification);

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _eventsReceived.TryTake(out EventReceivedEventArgs ev, -1);
                Assert.AreEqual(NotificationType.SPLIT_KILL, ev.Event.Type);
                Assert.AreEqual(1585868246622, ((SplitKillNotification)ev.Event).ChangeNumber);
                Assert.AreEqual("off", ((SplitKillNotification)ev.Event).DefaultTreatment);
                Assert.AreEqual("test-split", ((SplitKillNotification)ev.Event).SplitName);
                Assert.AreEqual(1, _connectedEvent.Count);

                eventSourceClient.Disconnect();
            }
        }

        [TestMethod]
        public void EventSourceClient_SegmentUpdateEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "id: 234234432\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_segments\",\"data\":\"{\\\"type\\\":\\\"SEGMENT_UPDATE\\\",\\\"changeNumber\\\":1585868933303,\\\"segmentName\\\":\\\"test-segment\\\"}\"}";
                httpClientMock.SSE_Channels_Response(notification);

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _eventsReceived.TryTake(out EventReceivedEventArgs ev, -1);
                Assert.AreEqual(NotificationType.SEGMENT_UPDATE, ev.Event.Type);
                Assert.AreEqual(1585868933303, ((SegmentChangeNotification)ev.Event).ChangeNumber);
                Assert.AreEqual("test-segment", ((SegmentChangeNotification)ev.Event).SegmentName);
                _connectedEvent.TryTake(out EventArgs e, -1);
                Assert.IsNotNull(e);
            }
        }

        [TestMethod]
        public void EventSourceClient_ControlEvent_StreamingPaused_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "id: 234234432\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"control_pri\",\"data\":\"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_PAUSED\\\"}\"}";
                httpClientMock.SSE_Channels_Response(notification);

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _eventsReceived.TryTake(out EventReceivedEventArgs ev, -1);
                Assert.AreEqual(NotificationType.CONTROL, ev.Event.Type);
                Assert.AreEqual(ControlType.STREAMING_PAUSED, ((ControlNotification)ev.Event).ControlType);
                Assert.AreEqual(1, _connectedEvent.Count);

                eventSourceClient.Disconnect();
            }
        }

        [TestMethod]
        public void EventSourceClient_ControlEvent_StreamingResumed_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "id: 234234432\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"control_pri\",\"data\":\"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_RESUMED\\\"}\"}";
                httpClientMock.SSE_Channels_Response(notification);

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _eventsReceived.TryTake(out EventReceivedEventArgs ev, -1);
                Assert.AreEqual(NotificationType.CONTROL, ev.Event.Type);
                Assert.AreEqual(ControlType.STREAMING_RESUMED, ((ControlNotification)ev.Event).ControlType);
                Assert.AreEqual(1, _connectedEvent.Count);

                eventSourceClient.Disconnect();
            }
        }

        [TestMethod]
        public void EventSourceClient_ControlEvent_StreamingDisabled_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "id: 234234432\nevent: message\ndata: {\"id\":\"jSOE7oGJWo:0:0\",\"clientId\":\"pri:ODc1NjQyNzY1\",\"timestamp\":1588254699236,\"encoding\":\"json\",\"channel\":\"control_pri\",\"data\":\"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"STREAMING_DISABLED\\\"}\"}";
                httpClientMock.SSE_Channels_Response(notification);

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _eventsReceived.TryTake(out EventReceivedEventArgs ev, -1);
                Assert.AreEqual(NotificationType.CONTROL, ev.Event.Type);
                Assert.AreEqual(ControlType.STREAMING_DISABLED, ((ControlNotification)ev.Event).ControlType);
                Assert.AreEqual(1, _connectedEvent.Count);

                eventSourceClient.Disconnect();
            }
        }

        [TestMethod]
        public void EventSourceClient_IncorrectFormat_ShouldReceiveError()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                httpClientMock.SSE_Channels_Response(
                    @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'content': {
                                'type': 'CONTROL', 
                                'controlType': 'test-control-type'
                            },
                            'name':'name-test'
                         }
                        }");

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _connectedEvent.TryTake(out EventArgs e, -1);
                Assert.IsNotNull(e);
                Assert.AreEqual(0, _eventsReceived.Count);

                eventSourceClient.Disconnect();
            }
        }

        [TestMethod]
        public void EventSourceClient_NotificationError_ShouldReceiveError()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "event: error\ndata: {\"message\":\"Token expired\",\"code\":40142,\"statusCode\":401,\"href\":\"https://help.ably.io/error/40142\"}\n\n";
                httpClientMock.SSE_Channels_Response(notification);

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _disconnectEvent.TryTake(out EventArgs e, -1);
                Assert.IsNotNull(e);
            }
        }

        [TestMethod]
        public void EventSourceClient_KeepAliveResponse()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                httpClientMock.SSE_Channels_Response(":keepalive\n\n");

                var keepAliveHandler = new KeepAliveHandler();
                var url = httpClientMock.GetUrl();
                _eventsReceived = new BlockingCollection<EventReceivedEventArgs>(new ConcurrentQueue<EventReceivedEventArgs>());
                _connectedEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());
                _disconnectEvent = new BlockingCollection<EventArgs>(new ConcurrentQueue<EventArgs>());

                var eventSourceClient = new EventSourceClient(backOffBase: 5, keepAliveHandler: keepAliveHandler);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ConnectedEvent += ConnectedEvent;
                eventSourceClient.DisconnectEvent += DisconnectEvent;
                eventSourceClient.Connect(url);

                _connectedEvent.TryTake(out EventArgs e, -1);
                Assert.IsNotNull(e);
                Thread.Sleep(1000);
                Assert.AreEqual(0, _eventsReceived.Count);

                eventSourceClient.Disconnect();
            }
        }

        #region Private Method
        private void EventReceived(object sender, EventReceivedEventArgs e)
        {
            _eventsReceived.TryAdd(e);
        }

        private void ConnectedEvent(object sender, EventArgs e)
        {
            _connectedEvent.TryAdd(e);
        }

        private void DisconnectEvent(object sender, EventArgs e)
        {
            _disconnectEvent.TryAdd(e);
        }
        #endregion
    }
}
