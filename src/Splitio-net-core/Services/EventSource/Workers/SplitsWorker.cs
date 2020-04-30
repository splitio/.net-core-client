using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;
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
            try
            {
                if (_queue != null)
                {
                    _log.Debug($"Add to queue: {changeNumber}");
                    _queue.TryAdd(changeNumber);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"AddToQueue: {ex.Message}");
            }
        }

        public void KillSplit(long changeNumber, string splitName, string defaultTreatment)
        {
            try
            {
                if (_queue != null)
                {
                    _log.Debug($"Kill Split: {splitName}, changeNumber: {changeNumber} and defaultTreatment: {defaultTreatment}");
                    _splitCache.Kill(changeNumber, splitName, defaultTreatment);
                    AddToQueue(changeNumber);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"KillSplit: {ex.Message}");
            }
        }

        public void Start()
        {
            try
            {
                _log.Debug("SplitsWorker starting ...");
                _queue = new BlockingCollection<long>(new ConcurrentQueue<long>());
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(() => Execute(), _cancellationTokenSource.Token);
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
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _queue?.Dispose();
                _queue = null;

                _log.Debug("SplitsWorker stoped ...");
            }
            catch (Exception ex)
            {
                _log.Error($"Stop: {ex.Message}");
            }
        }
        #endregion

        #region Private Mthods
        private void Execute()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    //Wait indefinitely until a segment is queued
                    if (_queue.TryTake(out long changeNumber, -1))
                    {
                        _log.Debug($"ChangeNumber dequeue: {changeNumber}");

                        if (changeNumber > _splitCache.GetChangeNumber())
                        {
                            _synchronizer.SynchronizeSplits();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Execute: {ex.Message}");
            }
        }
        #endregion
    }
}
