using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Client.Classes;
using System.Collections.Concurrent;

namespace Splitio_Tests.Unit_Tests.Client
{
    public class SplitClientForTesting : SplitClient
    {
        public SplitClientForTesting(ILog log, ConcurrentDictionary<string, ParsedSplit> splits) : base(log)
        {
            splitCache = new InMemorySplitCache(splits);
            manager = new SplitManager(splitCache);
        }

        public override void Destroy()
        {
        }
    }
}
