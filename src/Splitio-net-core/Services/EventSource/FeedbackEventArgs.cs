using System;

namespace Splitio.Services.EventSource
{
    public class SSEActionsEventArgs : EventArgs
    {
        public SSEClientActions Action { get; }

        public SSEActionsEventArgs(SSEClientActions action)
        {
            Action = action;
        }
    }
}
