using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;

namespace Splitio.Services.EventSource
{
    public class SSEHandler : ISSEHandler
    {
        private readonly ISplitLogger _log;
        private readonly IEventSourceClient _eventSourceClient;

        public event EventHandler<EventArgs> ConnectedEvent;
        public event EventHandler<EventArgs> DisconnectEvent;

        public SSEHandler(string sseUrl,
            ISplitLogger log,
            IEventSourceClient eventSourceClient = null)
        {
            _log = log ?? WrapperAdapter.GetLogger(typeof(SSEHandler));
            _eventSourceClient = eventSourceClient ?? new EventSourceClient(sseUrl);
        }

        #region Private Methods
        public void Start()
        {
            _eventSourceClient.EventReceived += EventReceived;
            _eventSourceClient.ConnectedEvent += OnConnected;
            _eventSourceClient.DisconnectEvent += OnDisconnect;
            _eventSourceClient.Connect();
        }

        public void Stop()
        {
            _eventSourceClient.Disconnect();
        }

        public void StartWorkers()
        {
            throw new System.NotImplementedException();
        }

        public void StopWorkers()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void EventReceived(object sender, EventReceivedEventArgs e)
        {
            
        }

        private void OnConnected(object sender, EventArgs e)
        {
            ConnectedEvent?.Invoke(this, e);
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            DisconnectEvent?.Invoke(this, e);
        }
        #endregion
    }
}
