using Common.Logging;
using Splitio.Services.Client.Classes;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ILog log) : base(log) { }

        public override void Destroy()
        {
        }
    }
}
