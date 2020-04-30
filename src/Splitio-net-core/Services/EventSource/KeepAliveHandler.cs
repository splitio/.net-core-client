using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.EventSource
{
    public class KeepAliveHandler : IKeepAliveHandler
    {
        private readonly object _clockLock = new object();
        private Stopwatch _clock;

        public event EventHandler<EventArgs> ReconnectEvent;

        #region Public Methods
        public void Start(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() => Run(cancellationToken), cancellationToken);
        }

        public void Run(CancellationToken cancellationToken)
        {
            _clock = new Stopwatch();
            _clock.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                var seconds = GetTimerValue() / 1000;

                if (seconds >= 70)
                {
                    OnReconnect(EventArgs.Empty);
                    _clock.Stop();
                    break;
                }
            }
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
        private long GetTimerValue()
        {
            lock (_clockLock)
            {
                return _clock.ElapsedMilliseconds;
            }
        }

        private void OnReconnect(EventArgs e)
        {
            ReconnectEvent?.Invoke(this, e);
        }
        #endregion
    }
}
