﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;
using System.Collections.Generic;
using System.Threading;

namespace Splitio_net_core.Integration_tests.EventSource
{
    [Ignore]
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
                httpClientMock.SSE_Channels_Response(
                    @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'data': {
                                'type': 'SPLIT_UPDATE', 
                                'changeNumber': 1254564
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

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(1, _eventsReceived.Count);
                var ev = _eventsReceived.Dequeue();
                Assert.AreEqual(NotificationType.SPLIT_UPDATE, ev.Event.Type);
                Assert.AreEqual(1254564, ((SplitUpdateEventData)ev.Event).ChangeNumber);
            }
        }

        [TestMethod]
        public void EventSourceClient_SplitKillEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                httpClientMock.SSE_Channels_Response(
                    @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'data': {
                                'type': 'SPLIT_KILL', 
                                'changeNumber': 1254564,
                                'defaultTreatment':'off',
                                'splitName': 'test-split'
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

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(1, _eventsReceived.Count);
                var ev = _eventsReceived.Dequeue();
                Assert.AreEqual(NotificationType.SPLIT_KILL, ev.Event.Type);
                Assert.AreEqual(1254564, ((SplitKillEventData)ev.Event).ChangeNumber);
                Assert.AreEqual("off", ((SplitKillEventData)ev.Event).DefaultTreatment);
                Assert.AreEqual("test-split", ((SplitKillEventData)ev.Event).SplitName);
            }
        }

        [TestMethod]
        public void EventSourceClient_SegmentUpdateEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                httpClientMock.SSE_Channels_Response(
                    @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'data': {
                                'type': 'SEGMENT_UPDATE', 
                                'changeNumber': 1254564,
                                'segmentName': 'test-segment'
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

                Assert.AreEqual(0, _errorsReceived.Count);
                Assert.AreEqual(1, _eventsReceived.Count);
                var ev = _eventsReceived.Dequeue();
                Assert.AreEqual(NotificationType.SEGMENT_UPDATE, ev.Event.Type);
                Assert.AreEqual(1254564, ((SegmentUpdateEventData)ev.Event).ChangeNumber);
                Assert.AreEqual("test-segment", ((SegmentUpdateEventData)ev.Event).SegmentName);
            }
        }

        [TestMethod]
        public void EventSourceClient_ControlEvent_ShouldReceiveEvent()
        {
            using (var httpClientMock = new HttpClientMock())
            {
                httpClientMock.SSE_Channels_Response(
                    @"{ 'event': 'message', 
                        'data': {
                            'id':'1',
                            'channel':'mauroc',
                            'data': {
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
