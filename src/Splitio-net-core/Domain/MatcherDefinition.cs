
namespace Splitio.Domain
{
    public class MatcherDefinition
    {
        public KeySelector keySelector { get; set; }
        public string matcherType { get; set; }
        public bool negate { get; set; }
        public UserDefinedSegmentData userDefinedSegmentMatcherData { get; set; }
        public WhitelistData whitelistMatcherData { get; set; }
        public UnaryNumericData unaryNumericMatcherData { get; set; }
        public BetweenData betweenMatcherData { get; set; }
        public DependencyData dependencyMatcherData { get; set; }
        public bool? booleanMatcherData { get; set; }
        public string stringMatcherData { get; set; }
    }
}
