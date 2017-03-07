
namespace Splitio.Domain
{
    public class MatcherDefinition
    {
        public KeySelector keySelector { get; set; }
        public MatcherTypeEnum? matcherType { get; set; }
        public bool negate { get; set; }
        public UserDefinedSegmentData userDefinedSegmentMatcherData { get; set; }
        public WhitelistData whitelistMatcherData { get; set; }
        public UnaryNumericData unaryNumericMatcherData { get; set; }
        public BetweenData betweenMatcherData { get; set; }
    }
}
