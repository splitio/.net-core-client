using System.Collections.Generic;

namespace Splitio.Domain
{
    public class ConditionDefinition
    {
        public string conditionType { get; set; }
        public MatcherGroupDefinition matcherGroup { get; set; }
        public List<PartitionDefinition> partitions { get; set; }
        public string label { get; set; }
    }
}
