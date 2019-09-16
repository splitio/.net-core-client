using System.Collections.Generic;

namespace Splitio.Domain
{
    public class Split : SplitBase
    {        
        public string status { get; set; }
        public List<ConditionDefinition> conditions { get; set; }
        public int? algo { get; set; }
        public int? trafficAllocationSeed { get; set; }
    }
}
