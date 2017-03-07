using System.Collections.Generic;

namespace Splitio.Domain
{
    public class SplitChangesResult
    {
        public long since { get; set; }
        public long till { get; set; }
        public List<Split> splits { get; set; }
    }
}
