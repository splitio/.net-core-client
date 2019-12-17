using Splitio.Services.Client.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Logger;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class LocalhostClientForTesting : LocalhostClient
    {
        public LocalhostClientForTesting(string filePath,
            ISplitLogger log = null,
            ISplitter splitter = null,
            bool isDestroyed = false) : base(filePath, log)
        {
            Destroyed = isDestroyed;
        }
    }
}
