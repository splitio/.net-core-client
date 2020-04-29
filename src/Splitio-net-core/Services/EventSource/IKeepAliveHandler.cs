using System;
using System.Threading;

namespace Splitio.Services.EventSource
{
    public interface IKeepAliveHandler
    {
        void Start(CancellationToken cancellationTokenSource);
        void Restart();

        event EventHandler<EventArgs> ReconnectEvent;
    }
}
