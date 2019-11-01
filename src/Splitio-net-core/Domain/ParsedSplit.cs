using System.Collections.Generic;

namespace Splitio.Domain
{
    public class ParsedSplit : SplitBase
    {
        public List<ConditionWithLogic> conditions { get; set; }
        public AlgorithmEnum algo { get; set; }
        public int trafficAllocationSeed { get; set; }
    }
}
