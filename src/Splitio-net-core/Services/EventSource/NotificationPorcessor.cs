using Splitio.Services.Logger;
using System;

namespace Splitio.Services.EventSource
{
    public class NotificationPorcessor : INotificationPorcessor
    {
        private readonly ISplitLogger _log;
        private readonly IEventSourceClient _eventSourceClient;

        public NotificationPorcessor(ISplitLogger log,
            IEventSourceClient eventSourceClient)
        {
            _log = log;
            _eventSourceClient = eventSourceClient;
        }

        public void StartClient()
        {
            throw new NotImplementedException();
        }

        public void StopClient()
        {
            throw new NotImplementedException();
        }
    }
}
