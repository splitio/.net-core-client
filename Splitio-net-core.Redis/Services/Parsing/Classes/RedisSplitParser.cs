using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Parsing;

namespace Splitio.Redis.Services.Parsing.Classes
{
    public class RedisSplitParser : SplitParser
    {
        public RedisSplitParser(ISegmentCache segmentsCache)
        {
            _segmentsCache = segmentsCache;
        }

        protected override IMatcher GetInSegmentMatcher(MatcherDefinition matcherDefinition, ParsedSplit parsedSplit)
        {
            var matcherData = matcherDefinition.userDefinedSegmentMatcherData;

            return new UserDefinedSegmentMatcher(matcherData.segmentName, _segmentsCache);
        }
    }
}
