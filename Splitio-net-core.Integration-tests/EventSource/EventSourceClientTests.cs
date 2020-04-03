using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;
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
        public void EventSourceClient_SplitUpdateEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "{\"id\":\"234234432\",\"event\":\"message\",\"data\":{\"id\":\"KXLEfWv-l4:0:0\",\"clientId\":\"3233424\",\"timestamp\":1585867724988,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_UPDATE\\\",\\\"changeNumber\\\":1585867723838}\"}}\n";
                httpClientMock.SSE_Channels_Response(notification);

                var url = httpClientMock.GetUrl();
                _eventsReceived = new Queue<EventReceivedEventArgs>();
                _errorsReceived = new Queue<ErrorReceivedEventArgs>();

                var eventSourceClient = new EventSourceClient(url, 10000);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ErrorReceived += ErrorReceived;

                Thread.Sleep(5000);

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(1, _eventsReceived.Count);
                var ev = _eventsReceived.Dequeue();
                Assert.AreEqual(NotificationType.SPLIT_UPDATE, ev.Event.Type);
                Assert.AreEqual(1585867723838, ((SplitChangeNotifiaction)ev.Event).ChangeNumber);
            }
        }

        [TestMethod]
        public void EventSourceClient_SplitKillEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "{\"id\":\"23423432\",\"event\":\"message\",\"data\":{\"id\":\"vJ0EW4_EZa:0:0\",\"clientId\":\"332432324\",\"timestamp\":1585868247781,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_splits\",\"data\":\"{\\\"type\\\":\\\"SPLIT_KILL\\\",\\\"changeNumber\\\":1585868246622,\\\"defaultTreatment\\\":\\\"off\\\",\\\"splitName\\\":\\\"test-split\\\"}\"}}\n";
                httpClientMock.SSE_Channels_Response(notification);

                var url = httpClientMock.GetUrl();
                _eventsReceived = new Queue<EventReceivedEventArgs>();
                _errorsReceived = new Queue<ErrorReceivedEventArgs>();

                var eventSourceClient = new EventSourceClient(url, 10000);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ErrorReceived += ErrorReceived;

                Thread.Sleep(5000);

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(1, _eventsReceived.Count);
                var ev = _eventsReceived.Dequeue();
                Assert.AreEqual(NotificationType.SPLIT_KILL, ev.Event.Type);
                Assert.AreEqual(1585868246622, ((SplitKillNotification)ev.Event).ChangeNumber);
                Assert.AreEqual("off", ((SplitKillNotification)ev.Event).DefaultTreatment);
                Assert.AreEqual("test-split", ((SplitKillNotification)ev.Event).SplitName);
            }
        }

        [TestMethod]
        public void EventSourceClient_SegmentUpdateEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "{\"id\":\"234432\",\"event\":\"message\",\"data\":{\"id\":\"rwlbcidVwD:0:0\",\"clientId\":\"234234234\",\"timestamp\":1585868933616,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_segments\",\"data\":\"{\\\"type\\\":\\\"SEGMENT_UPDATE\\\",\\\"changeNumber\\\":1585868933303,\\\"segmentName\\\":\\\"test-segment\\\"}\"}}\n";
                httpClientMock.SSE_Channels_Response(notification);

                var url = httpClientMock.GetUrl();
                _eventsReceived = new Queue<EventReceivedEventArgs>();
                _errorsReceived = new Queue<ErrorReceivedEventArgs>();

                var eventSourceClient = new EventSourceClient(url, 10000);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ErrorReceived += ErrorReceived;

                Thread.Sleep(5000);

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(1, _eventsReceived.Count);
                var ev = _eventsReceived.Dequeue();
                Assert.AreEqual(NotificationType.SEGMENT_UPDATE, ev.Event.Type);
                Assert.AreEqual(1585868933303, ((SegmentChangeNotification)ev.Event).ChangeNumber);
                Assert.AreEqual("test-segment", ((SegmentChangeNotification)ev.Event).SegmentName);
            }
        }

        [TestMethod]
        public void EventSourceClient_ControlEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "{\"id\":\"234432\",\"event\":\"message\",\"data\":{\"id\":\"rwlbcidVwD:0:0\",\"clientId\":\"234234234\",\"timestamp\":1585868933616,\"encoding\":\"json\",\"channel\":\"xxxx_xxxx_segments\",\"data\":\"{\\\"type\\\":\\\"CONTROL\\\",\\\"controlType\\\":\\\"test-control-type\\\"}\"}}\n";
                httpClientMock.SSE_Channels_Response(notification);

                var url = httpClientMock.GetUrl();
                _eventsReceived = new Queue<EventReceivedEventArgs>();
                _errorsReceived = new Queue<ErrorReceivedEventArgs>();

                var eventSourceClient = new EventSourceClient(url, 10000);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ErrorReceived += ErrorReceived;

                Thread.Sleep(5000);

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(1, _eventsReceived.Count);
                var ev = _eventsReceived.Dequeue();
                Assert.AreEqual(NotificationType.CONTROL, ev.Event.Type);
                Assert.AreEqual("test-control-type", ((ControlEventData)ev.Event).ControlType);
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

                var url = httpClientMock.GetUrl();
                _eventsReceived = new Queue<EventReceivedEventArgs>();
                _errorsReceived = new Queue<ErrorReceivedEventArgs>();

                var eventSourceClient = new EventSourceClient(url, 10000);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ErrorReceived += ErrorReceived;

                Thread.Sleep(5000);

                Assert.AreEqual(1, _errorsReceived.Count);
                Assert.AreEqual(0, _eventsReceived.Count);
            }
        }

        [TestMethod]
        public void EventSourceClient_NotificationError_ShouldReceiveError()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                var notification = "{\n\t\"error\":{\n\t\t\"message\":\"Token expired. (See https://help.fake.io/error/40142 for help.)\",\n\t\t\"code\":40142,\n\t\t\"statusCode\":401,\n\t\t\"href\":\"https://help.ably.io/error/40142\",\n\t\t\"serverId\":\"123123\"\n\t}\n}";
                httpClientMock.SSE_Channels_Response(notification);

                var url = httpClientMock.GetUrl();
                _eventsReceived = new Queue<EventReceivedEventArgs>();
                _errorsReceived = new Queue<ErrorReceivedEventArgs>();

                var eventSourceClient = new EventSourceClient(url, 10000);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ErrorReceived += ErrorReceived;

                Thread.Sleep(5000);

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(0, _eventsReceived.Count);
            }
        }

        [TestMethod]
        public void EventSourceClient_KeepAliveResponse()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                httpClientMock.SSE_Channels_Response("\n");

                var url = httpClientMock.GetUrl();
                _eventsReceived = new Queue<EventReceivedEventArgs>();
                _errorsReceived = new Queue<ErrorReceivedEventArgs>();

                var eventSourceClient = new EventSourceClient(url, 10000);
                eventSourceClient.EventReceived += EventReceived;
                eventSourceClient.ErrorReceived += ErrorReceived;

                Thread.Sleep(5000);

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(0, _eventsReceived.Count);
            }
        }

        #region Private Method
        private void EventReceived(object sender, EventReceivedEventArgs e)
        {
            _eventsReceived.Enqueue(e);
        }

        private void ErrorReceived(object sender, ErrorReceivedEventArgs e)
        {
            _errorsReceived.Enqueue(e);
        }
        #endregion
    }
}
