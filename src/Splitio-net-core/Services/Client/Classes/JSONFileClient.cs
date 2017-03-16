using log4net;
using log4net.Repository.Hierarchy;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Parsing.Classes;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.SplitFetcher.Classes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Splitio.Services.Client.Classes
{
    public class JSONFileClient:SplitClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JSONFileClient));

        public JSONFileClient(string splitsFilePath, string segmentsFilePath, ISegmentCache segmentCacheInstance = null, ISplitCache splitCacheInstance = null, ITreatmentLog treatmentLogInstance = null, bool isLabelsEnabled = true)
        {
            InitializeLogger();
            segmentCache = segmentCacheInstance ?? new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var segmentFetcher = new JSONFileSegmentFetcher(segmentsFilePath, segmentCache);
            var splitParser = new InMemorySplitParser(segmentFetcher, segmentCache);
            var splitChangeFetcher = new JSONFileSplitChangeFetcher(splitsFilePath);
            var splitChangesResult = splitChangeFetcher.Fetch(-1);
            var parsedSplits = new ConcurrentDictionary<string, ParsedSplit>();
            foreach (Split split in splitChangesResult.splits)
                parsedSplits.TryAdd(split.name, splitParser.Parse(split));         
            splitCache = splitCacheInstance ?? new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>(parsedSplits));
            treatmentLog = treatmentLogInstance;
            splitter = new Splitter();
            LabelsEnabled = isLabelsEnabled;
        }

        private void InitializeLogger()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository("splitio");
            log4net.Config.XmlConfigurator.Configure(hierarchy);
        }

        public void RemoveSplitFromCache(string splitName)
        {
            splitCache.RemoveSplit(splitName);
        }

        public void RemoveKeyFromSegmentCache(string segmentName, List<string> keys)
        {
            segmentCache.RemoveFromSegment(segmentName, keys);
        }
    }
}
