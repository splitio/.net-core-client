using System.Collections.Generic;

namespace Splitio.Domain
{
    public class SegmentChange
    {
        public string name { get; set; }
        public long since { get; set; }
        public long till { get; set; }
        public List<string> added { get; set; }
        public List<string> removed { get; set; }
    }
}
