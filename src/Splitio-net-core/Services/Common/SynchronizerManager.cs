using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public class SynchronizerManager : ISynchronizerManager
    {
        private readonly ISynchronizer _synchronizer;
        private readonly IPushManager _pushManager;
        private readonly ISplitLogger _log;

        public SynchronizerManager(ISynchronizer synchronizer,
            IPushManager pushManager,
            ISplitLogger log = null)
        {
            _synchronizer = synchronizer;
            _pushManager = pushManager;
            _log = log ?? WrapperAdapter.GetLogger(typeof(Synchronizer));
        }

        #region Public Methods
        public void Start()
        {
            StartPoll();
            // TODO: update this when add the config
            //StartStream();
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
            Task.Factory.StartNew(() =>
            {
                _synchronizer.SyncAll();
                _synchronizer.StartPeriodicDataRecording();
            });

            Task.Factory.StartNew(() =>
            {
                _pushManager.StartSse();
            });
        }
        #endregion
    }
}
