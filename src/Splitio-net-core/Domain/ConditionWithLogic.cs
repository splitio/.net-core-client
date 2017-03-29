using System.Collections.Generic;

namespace Splitio.Domain
{
    public class ConditionWithLogic
    {
        public ConditionType conditionType { get; set; }
        public CombiningMatcher matcher { get; set; }
        public List<PartitionDefinition> partitions { get; set; }
        public string label { get; set; }
    }
}
