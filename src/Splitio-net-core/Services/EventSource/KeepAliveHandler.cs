using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource
{
    public class KeepAliveHandler : IKeepAliveHandler
    {
        private readonly int _sseKeepaliveSeconds;

        private readonly object _clockLock = new object();
        private Stopwatch _clock;

        public event EventHandler<EventArgs> ReconnectEvent;

        public KeepAliveHandler(int sseKeepaliveSeconds = 70)
        {
            _sseKeepaliveSeconds = sseKeepaliveSeconds;
        }

        #region Public Methods
        public void Start(CancellationToken cancellationToken)
        {
            _clock = new Stopwatch();
            Task.Factory.StartNew(() => Run(cancellationToken), cancellationToken);
        }
        
        public void Stop()
        {
            lock (_clockLock)
            {
                _clock.Stop();
            }
        }

        public void Restart()
        {
            lock (_clockLock)
            {
                _clock.Restart();
                _clock.Start();
            }
        }
        #endregion

        #region Private Methods
        private void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var seconds = GetTimerValue() / 1000;

                if (seconds >= _sseKeepaliveSeconds)
                {
                    OnReconnect();
                    _clock.Stop();
                    return;
                }
            }
        }

        private long GetTimerValue()
        {
            lock (_clockLock)
            {
                return _clock.ElapsedMilliseconds;
            }
        }

        private void OnReconnect()
        {
            ReconnectEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
