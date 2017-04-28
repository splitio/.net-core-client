using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Parsing
{
    public class ContainsStringMatcher : BaseMatcher, IMatcher
    {
        private HashSet<string> itemsToCompare = new HashSet<string>();

        public ContainsStringMatcher(List<string> compareTo)
        {
            if (compareTo != null)
            {
                itemsToCompare.UnionWith(compareTo);
            }
        }

        public override bool Match(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                return false;
            }

            return itemsToCompare.Any(i => key.Contains(i));
        }

        public override bool Match(List<string> key)
        {
            return false;
        }

        public override bool Match(DateTime key)
        {
            return false;
        }

        public override bool Match(long key)
        {
            return false;
        }
    }
}