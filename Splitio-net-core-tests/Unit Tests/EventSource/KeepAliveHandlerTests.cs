using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.EventSource;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.EventSource
{
    [TestClass]
    public class KeepAliveHandlerTests
    {
        private IKeepAliveHandler _keepAliveHandler;
        private BlockingCollection<EventArgs> _reconnectedEvent;

        [TestMethod]
        public void Start_ShouldDispatchReconnectEvent()
        {
            // Arrange.
            var cancellationTokenKeepAlive = new CancellationTokenSource();
            _reconnectedEvent = new BlockingCollection<EventArgs>();            
            _keepAliveHandler = new KeepAliveHandler(sseKeepaliveSeconds: 5);
            _keepAliveHandler.ReconnectEvent += ProcessReconnectEvent;

            // Act.
            _keepAliveHandler.Start(cancellationTokenKeepAlive.Token);
            _keepAliveHandler.Restart();

            // Assert.
            _reconnectedEvent.TryTake(out EventArgs ev, -1);
            Assert.IsNotNull(ev);
        }

        [TestMethod]
        public void Start_ShouldNotDispatchReconnectEvent()
        {
            // Arrange.
            var cancellationTokenKeepAlive = new CancellationTokenSource();
            _reconnectedEvent = new BlockingCollection<EventArgs>();
            _keepAliveHandler = new KeepAliveHandler(sseKeepaliveSeconds: 10);
            _keepAliveHandler.ReconnectEvent += ProcessReconnectEvent;

            // Act.
            _keepAliveHandler.Start(cancellationTokenKeepAlive.Token);
            _keepAliveHandler.Restart();

            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(50);
                _keepAliveHandler.Restart();
            }

            // Assert.
            var result = _reconnectedEvent.TryTake(out EventArgs ev, 500);
            Assert.IsFalse(result);
        }

        private void ProcessReconnectEvent(object sender, EventArgs e)
        {
            _reconnectedEvent.TryAdd(e);
        }
    }
}
