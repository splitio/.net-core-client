using Splitio.Services.Client.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Domain
{
    public class CombiningMatcher
    {
        public CombinerEnum combiner { get; set; }
        public List<AttributeMatcher> delegates { get; set; }
        
        public bool Match(string key, Dictionary<string, object> attributes, ISplitClient splitClient = null)
        {
            if (delegates == null || delegates.Count() == 0)
            {
                return false;
            }

            switch (combiner)
            {
                case CombinerEnum.AND:
                default:
                    return delegates.All(matcher => matcher.Match(key, attributes, splitClient));
            }
        }
    }
}
