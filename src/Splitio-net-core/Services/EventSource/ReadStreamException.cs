using System;

namespace Splitio.Services.EventSource
{
    public class ReadStreamException : Exception
    {
        public SSEClientActions Action { get; set; }

        public ReadStreamException(SSEClientActions action, string message)
            : base(message)
        {
            Action = action;
        }
    }
}
