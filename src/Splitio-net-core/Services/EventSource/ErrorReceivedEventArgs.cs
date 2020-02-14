using System;

namespace Splitio.Services.EventSource
{
    public class ErrorReceivedEventArgs : EventArgs
    {
        public string Message { get; }

        public ErrorReceivedEventArgs(string message)
        {
            Message = message;
        }
    }
}
