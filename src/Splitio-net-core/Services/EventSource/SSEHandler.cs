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
        private readonly string _streaminServiceUrl;

        private IEventSourceClient _eventSourceClient;

        public event EventHandler<FeedbackEventArgs> ConnectedEvent;
        public event EventHandler<FeedbackEventArgs> DisconnectEvent;

        public SSEHandler(string streaminServiceUrl,
            ISplitsWorker splitsWorker,
            ISegmentsWorker segmentsWorker,
            INotificationProcessor notificationPorcessor,
            ISplitLogger log = null,
            IEventSourceClient eventSourceClient = null)
        {
            _streaminServiceUrl = streaminServiceUrl;
            _splitsWorker = splitsWorker;
            _segmentsWorker = segmentsWorker;
            _notificationPorcessor = notificationPorcessor;
            _log = log ?? WrapperAdapter.GetLogger(typeof(SSEHandler));
            _eventSourceClient = eventSourceClient;
        }

        #region Private Methods
        public void Start(string token, string channels)
        {
            try
            {
                _log.Debug($"SSE Handler starting...");
                var url = $"{_streaminServiceUrl}?channels={channels}&v=1.1&accessToken={token}";
                _eventSourceClient = _eventSourceClient ?? new EventSourceClient(url);

                _eventSourceClient.EventReceived += EventReceived;
                _eventSourceClient.ConnectedEvent += OnConnected;
                _eventSourceClient.DisconnectEvent += OnDisconnect;
                _eventSourceClient.Connect();                
            }
            catch (Exception ex)
            {
                _log.Error($"Start: {ex.Message}");
            }
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
                _log.Error($"Stop: {ex.Message}");
            }
        }
        #endregion

        #region Private Methods
        private void EventReceived(object sender, EventReceivedEventArgs e)
        {
            _log.Debug($"Event received {e.Event}");
            _notificationPorcessor.Proccess(e.Event);
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

        private void StartWorkers()
        {
            _splitsWorker.Start();
            _segmentsWorker.Start();
        }

        private void StopWorkers()
        {
            _splitsWorker.Stop();
            _segmentsWorker.Stop();
        }
        #endregion
    }
}
