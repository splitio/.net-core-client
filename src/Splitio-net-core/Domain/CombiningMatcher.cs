using Splitio.Services.Evaluator;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Domain
{
    public class CombiningMatcher
    {
        public CombinerEnum combiner { get; set; }
        public List<AttributeMatcher> delegates { get; set; }
        
        public virtual bool Match(Key key, Dictionary<string, object> attributes, IEvaluator evaluator = null)
        {
            if (delegates == null || delegates.Count() == 0)
            {
                return false;
            }

            switch (combiner)
            {
                case CombinerEnum.AND:
                default:
                    return delegates.All(matcher => matcher.Match(key, attributes, evaluator));
            }
        }
    }
}
