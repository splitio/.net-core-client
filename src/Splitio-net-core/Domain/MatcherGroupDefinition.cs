using System.Collections.Generic;

namespace Splitio.Domain
{
    public class MatcherGroupDefinition
    {
        public string combiner { get; set; }
        public List<MatcherDefinition> matchers {get; set;}
    }
}
