using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource.Workers
{
    public class SplitsWorker : ISplitsWorker
    {
        private readonly ISplitLogger _log;
        private readonly ISplitCache _splitCache;
        private readonly ISynchronizer _synchronizer;

        private BlockingCollection<long> _queue;
        private CancellationTokenSource _cancellationTokenSource;

        public SplitsWorker(ISplitCache splitCache,
            ISynchronizer synchronizer,
            ISplitLogger log = null)
        {
            _splitCache = splitCache;
            _synchronizer = synchronizer;
            _log = log ?? WrapperAdapter.GetLogger(typeof(SplitsWorker));
        }

        #region Public Methods
        public void AddToQueue(long changeNumber)
        {
            if (_queue != null)
            {
                _queue.TryAdd(changeNumber);
            }
        }

        public void KillSplit(long changeNumber, string splitName, string defaultTreatment)
        {
            if (_queue != null)
            {
                _splitCache.Kill(changeNumber, splitName, defaultTreatment);
                AddToQueue(changeNumber);
            }
        }

        public void Start()
        {
            _queue = new BlockingCollection<long>(new ConcurrentQueue<long>());
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => Execute(), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _queue.Dispose();
            _queue = null;
        }
        #endregion

        #region Private Mthods
        private void Execute()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                //Wait indefinitely until a segment is queued
                if (_queue.TryTake(out long changeNumber, -1))
                {
                    _log.Debug($"ChangeNumber dequeue: {changeNumber}");

                    if (changeNumber > _splitCache.GetChangeNumber())
                    {
                        _synchronizer.SynchorizeSplits();
                    }
                }
            }
        }
        #endregion
    }
}
