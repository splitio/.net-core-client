using System;

namespace Splitio.Services.EventSource
{
    public class FeedbackEventArgs : EventArgs
    {
        public bool IsConnected { get; }

        public FeedbackEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
