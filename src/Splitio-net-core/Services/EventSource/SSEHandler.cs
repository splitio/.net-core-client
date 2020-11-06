using Splitio.Services.EventSource.Workers;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;

namespace Splitio.Services.EventSource
{
    public class SSEHandler : ISSEHandler
    {
        private readonly ISplitLogger _log;
        private readonly ISplitsWorker _splitsWorker;
        private readonly ISegmentsWorker _segmentsWorker;
        private readonly INotificationProcessor _notificationPorcessor;
        private readonly INotificationManagerKeeper _notificationManagerKeeper;
        private readonly string _streaminServiceUrl;

        private IEventSourceClient _eventSourceClient;

        public event EventHandler<FeedbackEventArgs> ConnectedEvent;
        public event EventHandler<FeedbackEventArgs> DisconnectEvent;

        public SSEHandler(string streaminServiceUrl,
            ISplitsWorker splitsWorker,
            ISegmentsWorker segmentsWorker,
            INotificationProcessor notificationPorcessor,
            INotificationManagerKeeper notificationManagerKeeper,
            ISplitLogger log = null,
            IEventSourceClient eventSourceClient = null)
        {
            _streaminServiceUrl = streaminServiceUrl;
            _splitsWorker = splitsWorker;
            _segmentsWorker = segmentsWorker;
            _notificationPorcessor = notificationPorcessor;
            _notificationManagerKeeper = notificationManagerKeeper;
            _log = log ?? WrapperAdapter.GetLogger(typeof(SSEHandler));
            _eventSourceClient = eventSourceClient;

            _eventSourceClient.EventReceived += EventReceived;
            _eventSourceClient.ConnectedEvent += OnConnected;
            _eventSourceClient.DisconnectEvent += OnDisconnect;
        }

        #region Private Methods
        public bool Start(string token, string channels)
        {
            try
            {
                _log.Debug($"SSE Handler starting...");
                var url = $"{_streaminServiceUrl}?channels={channels}&v=1.1&accessToken={token}";

                return _eventSourceClient.ConnectAsync(url);
            }
            catch (Exception ex)
            {
                _log.Error($"SSE Handler Start: {ex.Message}");
            }

            return false;
        }

        public void Stop()
        {
            try
            {
                if (_eventSourceClient != null)
                {
                    _eventSourceClient.Disconnect();
                    _log.Debug($"SSE Handler stoped...");
                }
            }
            catch (Exception ex)
            {
                _log.Debug($"SSE Handler Stop: {ex.Message}");
            }
        }

        public void StartWorkers()
        {
            _splitsWorker.Start();
            _segmentsWorker.Start();
        }

        public void StopWorkers()
        {
            _splitsWorker.Stop();
            _segmentsWorker.Stop();
        }
        #endregion

        #region Private Methods
        private void EventReceived(object sender, EventReceivedEventArgs e)
        {
            _log.Debug($"Event received {e.Event}");

            if (e.Event.Type == NotificationType.OCCUPANCY || e.Event.Type == NotificationType.CONTROL)
            {
                _notificationManagerKeeper.HandleIncomingEvent(e.Event);
            }
            else
            {
                _notificationPorcessor.Proccess(e.Event);
            }
        }

        private void OnConnected(object sender, FeedbackEventArgs e)
        {
            StartWorkers();
            ConnectedEvent?.Invoke(this, e);
        }

        private void OnDisconnect(object sender, FeedbackEventArgs e)
        {
            StopWorkers();
            DisconnectEvent?.Invoke(this, e);
        }
        #endregion
    }
}
