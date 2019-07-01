using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Parsing.Classes;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using Splitio.Services.SplitFetcher.Classes;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Splitio.Services.Client.Classes
{
    public class JSONFileClient : SplitClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JSONFileClient));

        public JSONFileClient(string splitsFilePath, 
            string segmentsFilePath, 
            ILog log,
            ISegmentCache segmentCacheInstance = null, 
            ISplitCache splitCacheInstance = null, 
            IListener<KeyImpression> treatmentLogInstance = null,
            bool isLabelsEnabled = true,
            IListener<WrappedEvent> _eventListener = null) : base(log)
        {
            segmentCache = segmentCacheInstance ?? new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var segmentFetcher = new JSONFileSegmentFetcher(segmentsFilePath, segmentCache);
            var splitParser = new InMemorySplitParser(segmentFetcher, segmentCache);
            var splitChangeFetcher = new JSONFileSplitChangeFetcher(splitsFilePath);
            var task = splitChangeFetcher.Fetch(-1);
            task.Wait();

            var splitChangesResult = task.Result;
            var parsedSplits = new ConcurrentDictionary<string, ParsedSplit>();
            foreach (Split split in splitChangesResult.splits)
            {
                parsedSplits.TryAdd(split.name, splitParser.Parse(split));
            }

            splitCache = splitCacheInstance ?? new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>(parsedSplits));
            impressionListener = treatmentLogInstance;
            splitter = new Splitter();
            LabelsEnabled = isLabelsEnabled;

            eventListener = _eventListener;
            
            _blockUntilReadyService = new BlockUntilReadyService();
            manager = new SplitManager(splitCache, _blockUntilReadyService, log);

            ApiKey = "localhost";
        }

        public void RemoveSplitFromCache(string splitName)
        {
            splitCache.RemoveSplit(splitName);
        }

        public void RemoveKeyFromSegmentCache(string segmentName, List<string> keys)
        {
            segmentCache.RemoveFromSegment(segmentName, keys);
        }

        public override void Destroy()
        {
            if (!Destroyed)
            {
                splitCache.Clear();
                segmentCache.Clear();
                base.Destroy();
            }
        }

        public override void BlockUntilReady(int blockMilisecondsUntilReady)
        {
            _blockUntilReadyService.BlockUntilReady(blockMilisecondsUntilReady);
        }
    }
}
