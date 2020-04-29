using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
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
            ISplitLogger log = null)
        {
            _streamingEnabled = streamingEnabled;
            _synchronizer = synchronizer;
            _pushManager = pushManager;
            _sseHandler = sseHandler;
            _log = log ?? WrapperAdapter.GetLogger(typeof(Synchronizer));

            _sseHandler.ConnectedEvent += OnProcessFeedbackSSE;
            _sseHandler.DisconnectEvent += OnProcessFeedbackSSE;
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
            _synchronizer.StopPeriodicDataRecording();
            _pushManager.StopSse();
        }
        #endregion

        #region Private Methods
        private void StartPoll()
        {
            _synchronizer.StartPeriodicFetching();
            _synchronizer.StartPeriodicDataRecording();
        }

        private void StartStream()
        {
            _synchronizer.StartPeriodicDataRecording();
            Task.Factory.StartNew(() => { _synchronizer.SyncAll(); });
            Task.Factory.StartNew(() => { _pushManager.StartSse(); });
        }

        private void OnProcessFeedbackSSE(object sender, FeedbackEventArgs e)
        {
            if (e.IsConnected)
            {
                _synchronizer.StopPeriodicFetching();
                _synchronizer.SyncAll();
            }
            else
            {
                _synchronizer.StartPeriodicFetching();
            }
        }
        #endregion
    }
}
