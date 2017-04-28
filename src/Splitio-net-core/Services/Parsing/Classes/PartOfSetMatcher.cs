using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Parsing
{
    public class PartOfSetMatcher : BaseMatcher, IMatcher
    {
        private HashSet<string> itemsToCompare = new HashSet<string>();

        public PartOfSetMatcher(List<string> compareTo)
        {
            if (compareTo != null)
            {
                itemsToCompare.UnionWith(compareTo);
            }
        }

        public override bool Match(List<string> key)
        {
            if (key == null || key.Count == 0)
            {
                return false;
            }

            return key.All(k => itemsToCompare.Contains(k));
        }

        public override bool Match(string key)
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