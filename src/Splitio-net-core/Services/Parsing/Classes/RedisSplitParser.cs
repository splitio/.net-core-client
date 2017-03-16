using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;

namespace Splitio.Services.Parsing.Classes
{
    public class RedisSplitParser : SplitParser
    {
        public RedisSplitParser(ISegmentCache segmentsCache)
        {
            this.segmentsCache = segmentsCache;
        }

        protected override IMatcher GetInSegmentMatcher(MatcherDefinition matcherDefinition, ParsedSplit parsedSplit)
        {
            var matcherData = matcherDefinition.userDefinedSegmentMatcherData;
            return new UserDefinedSegmentMatcher(matcherData.segmentName, segmentsCache);
        }
    }
}
