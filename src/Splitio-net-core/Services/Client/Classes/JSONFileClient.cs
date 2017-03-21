using NLog;
using NLog.Config;
using NLog.Targets;
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

namespace Splitio.Services.Client.Classes
{
    public class JSONFileClient:SplitClient
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(JSONFileClient).ToString());

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
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);
            fileTarget.FileName = @"Logs\split-sdk.log";
            fileTarget.Layout = "%date %level %logger - %message%newline";
            fileTarget.ConcurrentWrites = true;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Date;
            LogManager.Configuration = config;
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
