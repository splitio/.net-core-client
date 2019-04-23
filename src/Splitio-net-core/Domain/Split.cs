using System.Collections.Generic;

namespace Splitio.Domain
{
    public class Split: SplitBase
    {
        public string name { get; set; }
        public int seed { get; set; }
        public string status { get; set; }
        public bool killed { get; set; }
        public string defaultTreatment { get; set; }
        public List<ConditionDefinition> conditions { get; set; }
        public long changeNumber { get; set; }
        public string trafficTypeName { get; set; }
        public int? algo { get; set; }
        public int trafficAllocation { get; set; }
        public int? trafficAllocationSeed { get; set; }
        public string configurations { get; set; }
    }
}
