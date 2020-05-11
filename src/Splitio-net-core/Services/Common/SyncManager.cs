using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public class SyncManager : ISyncManager
    {
        private readonly bool _streamingEnabled;
        private readonly ISynchronizer _synchronizer;
        private readonly IPushManager _pushManager;
        private readonly ISSEHandler _sseHandler;
        private readonly ISplitLogger _log;

        public SyncManager(bool streamingEnabled,
            ISynchronizer synchronizer,
            IPushManager pushManager,
            ISSEHandler sseHandler,
            INotificationManagerKeeper notificationManagerKeeper,
            ISplitLogger log = null)
        {
            _streamingEnabled = streamingEnabled;
            _synchronizer = synchronizer;
            _pushManager = pushManager;
            _sseHandler = sseHandler;
            _log = log ?? WrapperAdapter.GetLogger(typeof(Synchronizer));

            _sseHandler.ConnectedEvent += OnProcessFeedbackSSE;
            _sseHandler.DisconnectEvent += OnProcessFeedbackSSE;
            _sseHandler.DisconnectEvent += OnProcessFeedbackSSE;
            notificationManagerKeeper.OccupancyEvent += OnOccupancyEvent;
            notificationManagerKeeper.PushShutdownEvent += OnPushShutdownEvent;
        }

        #region Public Methods
        public void Start()
        {
            if (_streamingEnabled)
            {
                StartStream();
            }
            else
            {
                StartPoll();
            }
        }

        public void Shutdown()
        {
            _synchronizer.StopPeriodicFetching();
            _synchronizer.ClearFetchersCache();
            _synchronizer.StopPeriodicDataRecording();
            _pushManager.StopSse();
        }
        #endregion

        #region Private Methods
        private void StartPoll()
        {
            _log.Debug("Starting push mode ...");
            _synchronizer.StartPeriodicFetching();
            _synchronizer.StartPeriodicDataRecording();
        }

        private void StartStream()
        {
            _log.Debug("Starting push mode ...");

            _synchronizer.StartPeriodicDataRecording();
            Task.Factory.StartNew(() => { _synchronizer.SyncAll(); });
            _pushManager.StartSse();
        }

        private void OnProcessFeedbackSSE(object sender, FeedbackEventArgs e)
        {
            if (e.IsConnected)
            {
                _synchronizer.StopPeriodicFetching();
                _synchronizer.SyncAll();
            }
            else if (e.Reconnect)
            {
                Task.Factory.StartNew(() => { _synchronizer.SyncAll(); });
                _pushManager.StartSse();
            }
            else
            {
                _synchronizer.StartPeriodicFetching();
            }
        }

        private void OnOccupancyEvent(object sender, OccupancyEventArgs e)
        {
            if (e.PublisherAvailable)
            {
                _synchronizer.StopPeriodicFetching();
                _synchronizer.SyncAll();
                _sseHandler.StartWorkers();
            }
            else
            {
                _sseHandler.StopWorkers();
                _synchronizer.StartPeriodicFetching();
            }
        }

        private void OnPushShutdownEvent(object sender, EventArgs e)
        {
            _pushManager.StopSse();
        }
        #endregion
    }
}
