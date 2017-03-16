using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.SegmentFetcher.Interfaces;

namespace Splitio.Services.Parsing.Classes
{
    public class InMemorySplitParser : SplitParser
    {
        private readonly ISegmentFetcher segmentFetcher;

        public InMemorySplitParser(ISegmentFetcher segmentFetcher, ISegmentCache segmentsCache)
        {
            this.segmentFetcher = segmentFetcher;
            this.segmentsCache = segmentsCache;
        }

        protected override IMatcher GetInSegmentMatcher(MatcherDefinition matcherDefinition, ParsedSplit parsedSplit)
        {
            var matcherData = matcherDefinition.userDefinedSegmentMatcherData;
            segmentFetcher.InitializeSegment(matcherData.segmentName);
            return new UserDefinedSegmentMatcher(matcherData.segmentName, segmentsCache);
        }
    }
}
