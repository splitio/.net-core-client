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

        private bool _streamingConnected;

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

            _sseHandler.ActionEvent += OnProcessFeedbackSSE;
            notificationManagerKeeper.ActionEvent += OnProcessFeedbackSSE;
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
            _log.Debug("Starting push mode...");

            _synchronizer.StartPeriodicDataRecording();
            _synchronizer.SyncAll();
            Task.Factory.StartNew(async () =>
            {
                if (!await _pushManager.StartSse())
                {
                    _synchronizer.StartPeriodicFetching();
                }
            });
        }

        private void OnProcessFeedbackSSE(object sender, SSEActionsEventArgs e)
        {
            switch (e.Action)
            {
                case SSEClientActions.CONNECTED:
                    ProcessConnected();
                    break;
                case SSEClientActions.DISCONNECT:
                case SSEClientActions.RETRYABLE_ERROR:
                    ProcessDisconnect(retry: true);
                    break;
                case SSEClientActions.NONRETRYABLE_ERROR:
                    ProcessDisconnect(retry: false);
                    break;
                case SSEClientActions.SUBSYSTEM_DOWN:
                    ProcessSubsystemDown();
                    break;
                case SSEClientActions.SUBSYSTEM_READY:
                    ProcessSubsystemReady();
                    break;
                case SSEClientActions.SUBSYSTEM_OFF:
                    ProcessSubsystemOff();
                    break;
            }
        }

        private void ProcessConnected()
        {
            if (_streamingConnected)
            {
                _log.Debug("Streaming already connected.");
                return;
            }

            _streamingConnected = true;
            _sseHandler.StartWorkers();
            _synchronizer.SyncAll();
            _synchronizer.StopPeriodicFetching();
        }

        private void ProcessDisconnect(bool retry)
        {
            if (!_streamingConnected)
            {
                _log.Debug("Streaming already disconnected.");
                return;
            }

            _streamingConnected = false;
            _sseHandler.StopWorkers();
            _synchronizer.SyncAll();
            _synchronizer.StartPeriodicFetching();

            if (retry)
            {
                _pushManager.StartSse();
            }
        }

        private void ProcessSubsystemDown()
        {
            _sseHandler.StopWorkers();
            _synchronizer.StartPeriodicFetching();
        }

        private void ProcessSubsystemReady()
        {
            _synchronizer.StopPeriodicFetching();
            _synchronizer.SyncAll();
            _sseHandler.StartWorkers();
        }

        private void ProcessSubsystemOff()
        {
            _pushManager.StopSse();
        }
        #endregion
    }
}
