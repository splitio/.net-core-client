using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ILog log, IListener<WrappedEvent> eventListener) 
            : base(log)
        {
            this.eventListener = eventListener;
        }

        public override void Destroy()
        {
        }
    }
}
