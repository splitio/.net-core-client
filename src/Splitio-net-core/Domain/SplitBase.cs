using System.Collections.Generic;

namespace Splitio.Domain
{
    public abstract class SplitBase
    {
        public string name { get; set; }
        public int seed { get; set; }
        public bool killed { get; set; }
        public string defaultTreatment { get; set; }
        public long changeNumber { get; set; }
        public string trafficTypeName { get; set; }
        public int trafficAllocation { get; set; }
        public Dictionary<string, string> configurations { get; set; }
    }
}
