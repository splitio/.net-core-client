using System;

namespace Splitio.Services.EventSource
{
    public class FeedbackEventArgs : EventArgs
    {
        public bool IsConnected { get; }
        public bool Reconnect { get; } 

        public FeedbackEventArgs(bool isConnected, bool reconnect = false)
        {
            IsConnected = isConnected;
            Reconnect = reconnect;
        }
    }
}
