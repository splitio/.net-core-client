using Splitio.Services.Logger;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource.Workers
{
    public abstract class Worker<T> : IWorker<T>
    {
        private readonly string _workerName;
        protected readonly ISplitLogger _log;

        protected BlockingCollection<T> _queue;
        protected CancellationTokenSource _cancellationTokenSource;
        protected bool _running;

        public Worker(ISplitLogger log, string workerName)
        {
            _log = log;
            _workerName = workerName;
        }

        public void AddToQueue(T element)
        {
            try
            {
                if (_queue != null)
                {
                    _log.Debug($"Add to {_workerName} queue: {element.ToString()}");
                    _queue.TryAdd(element);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"{_workerName} AddToQueue: {ex.Message}");
            }
        }

        public void Start()
        {
            try
            {
                if (_running)
                {
                    _log.Error($"{_workerName} Worker already running.");
                    return;
                }

                _log.Debug($"{_workerName} worker starting ...");
                _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(() => Execute(), _cancellationTokenSource.Token);
                _running = true;
            }
            catch (Exception ex)
            {
                _log.Error($"{_workerName} Start: {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                if (!_running)
                {
                    _log.Error($"{_workerName} Worker not running.");
                    return;
                }

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _queue?.Dispose();
                _queue = null;

                _log.Debug($"{_workerName} Worker stoped ...");
                _running = false;
            }
            catch (Exception ex)
            {
                _log.Error($"Stop: {ex.Message}");
            }
        }

        private void Execute()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    //Wait indefinitely until a element in the queued
                    if (_queue.TryTake(out T element, -1))
                    {
                        _log.Debug($"{_workerName} Dequeue: {element.ToString()}");

                        ForceRefreshAsync(element);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Execute: {ex.Message}");
            }
        }

        protected abstract void ForceRefreshAsync(T element);
    }
}
