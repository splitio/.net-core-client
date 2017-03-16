using System.Collections.Generic;

namespace Splitio.Domain
{
    public class MatcherGroupDefinition
    {
        public CombinerEnum combiner { get; set; }
        public List<MatcherDefinition> matchers {get; set;}
    }
}
