using System;

namespace Splitio.Services.EventSource
{
    public class ReadStreamException : Exception
    {
        public bool ReconnectEventSourveClient { get; set; }

        public ReadStreamException(bool reconnect, string message)
            : base(message)
        {
            ReconnectEventSourveClient = reconnect;
        }
    }
}
