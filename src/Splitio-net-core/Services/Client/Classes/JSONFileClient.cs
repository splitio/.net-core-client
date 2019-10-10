using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Logger;
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
        public JSONFileClient(string splitsFilePath,
            string segmentsFilePath,
            ISplitLogger log = null,
            ISegmentCache segmentCacheInstance = null,
            ISplitCache splitCacheInstance = null,
            IListener<KeyImpression> treatmentLogInstance = null,
            bool isLabelsEnabled = true,
            IListener<WrappedEvent> _eventListener = null,
            ITrafficTypeValidator trafficTypeValidator = null) : base(GetLogger(log))
        {
            segmentCache = segmentCacheInstance ?? new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var segmentFetcher = new JSONFileSegmentFetcher(segmentsFilePath, segmentCache);            
            var splitChangeFetcher = new JSONFileSplitChangeFetcher(splitsFilePath);
            var task = splitChangeFetcher.Fetch(-1);
            task.Wait();
            
            var splitChangesResult = task.Result;
            var parsedSplits = new ConcurrentDictionary<string, ParsedSplit>();

            _splitParser = new InMemorySplitParser(segmentFetcher, segmentCache);

            foreach (var split in splitChangesResult.splits)
            {
                parsedSplits.TryAdd(split.name, _splitParser.Parse(split));
            }

            splitCache = splitCacheInstance ?? new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>(parsedSplits));
            impressionListener = treatmentLogInstance;
            LabelsEnabled = isLabelsEnabled;

            eventListener = _eventListener;
            _trafficTypeValidator = trafficTypeValidator;
            
            _blockUntilReadyService = new NoopBlockUntilReadyService();
            manager = new SplitManager(splitCache, _blockUntilReadyService, log);

            ApiKey = "localhost";

            BuildEvaluator(log);
        }

        #region Public Methods
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
        #endregion

        #region Private Methods
        private static ISplitLogger GetLogger(ISplitLogger splitLogger = null)
        {
            return splitLogger ?? WrapperAdapter.GetLogger(typeof(JSONFileClient));
        }
        #endregion
    }
}
