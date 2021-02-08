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
        private readonly static int MaxRetriesAllowed = 10;

        private readonly ISplitLogger _log;
        private readonly ISegmentCache _segmentCache;
        private readonly ISynchronizer _synchronizer;
        private readonly BlockingCollection<SegmentQueueDto> _queue;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _running;

        public SegmentsWorker(ISegmentCache segmentCache,
            ISynchronizer synchronizer,
            ISplitLogger log = null)
        {
            _segmentCache = segmentCache;
            _synchronizer = synchronizer;
            _log = log ?? WrapperAdapter.GetLogger(typeof(SegmentsWorker));
            _queue = new BlockingCollection<SegmentQueueDto>(new ConcurrentQueue<SegmentQueueDto>());
        }

        #region Public Methods
        public void AddToQueue(long changeNumber, string segmentName)
        {
            try
            {
                if (!_running)
                {
                    _log.Error("Segments Worker not running.");
                    return;
                }

                _log.Debug($"Add to queue: {segmentName} - {changeNumber}");
                _queue.TryAdd(new SegmentQueueDto { ChangeNumber = changeNumber, SegmentName = segmentName });
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
                if (_running)
                {
                    _log.Error("Segments Worker already running.");
                    return;
                }

                _log.Debug($"Segments worker starting ...");
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(() => Execute(), _cancellationTokenSource.Token);
                _running = true;
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
                if (!_running)
                {
                    _log.Error("Segments Worker not running.");
                    return;
                }

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                _log.Debug($"Segments worker stoped ...");
                _running = false;
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
                    // Wait indefinitely until a segment is queued
                    if (_queue.TryTake(out SegmentQueueDto segment, -1))
                    {
                        _log.Debug($"Segment dequeue: {segment.SegmentName}");

                        var attempt = 0;

                        while (segment.ChangeNumber > _segmentCache.GetChangeNumber(segment.SegmentName) && (attempt < MaxRetriesAllowed))
                        {
                            await _synchronizer.SynchronizeSegment(segment.SegmentName);
                            attempt++;
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
