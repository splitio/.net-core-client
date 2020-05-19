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
    public class SegmentsWorker : ISegmentsWorker
    {
        private readonly ISplitLogger _log;
        private readonly ISegmentCache _segmentCache;
        private readonly ISynchronizer _synchronizer;

        private BlockingCollection<SegmentQueueDto> _queue;
        private CancellationTokenSource _cancellationTokenSource;

        public SegmentsWorker(ISegmentCache segmentCache,
            ISynchronizer synchronizer,
            ISplitLogger log = null)
        {
            _segmentCache = segmentCache;
            _synchronizer = synchronizer;
            _log = log ?? WrapperAdapter.GetLogger(typeof(SegmentsWorker));            
        }

        #region Public Methods
        public void AddToQueue(long changeNumber, string segmentName)
        {
            try
            {
                if (_queue != null)
                {
                    _log.Debug($"Add to queue: {segmentName} - {changeNumber}");
                    _queue.TryAdd(new SegmentQueueDto { ChangeNumber = changeNumber, SegmentName = segmentName });
                }
            }
            catch (Exception ex)
            {
                _log.Error($"AddToQueue: {ex.Message}");
            }
        }

        public void Start()
        {
            try
            {
                _log.Debug($"Segments worker starting ...");
                _queue = new BlockingCollection<SegmentQueueDto>(new ConcurrentQueue<SegmentQueueDto>());
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
                _log.Debug($"Segments worker stoped ...");
            }
            catch (Exception ex)
            {
                _log.Error($"Stop: {ex.Message}");
            }
        }
        #endregion

        #region Private Methods
        public async void Execute()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    //Wait indefinitely until a segment is queued
                    if (_queue.TryTake(out SegmentQueueDto segment, -1))
                    {
                        _log.Debug($"Segment dequeue: {segment.SegmentName}");

                        if (segment.ChangeNumber > _segmentCache.GetChangeNumber(segment.SegmentName))
                        {
                            await _synchronizer.SynchronizeSegment(segment.SegmentName);
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
