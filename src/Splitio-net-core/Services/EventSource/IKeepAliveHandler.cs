using System;
using System.Threading;

namespace Splitio.Services.EventSource
{
    public interface IKeepAliveHandler
    {
        void Start(CancellationToken cancellationTokenSource);
        void Restart();
        void Stop();

        event EventHandler<EventArgs> ReconnectEvent;
    }
}
