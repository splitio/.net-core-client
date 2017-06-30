using Splitio.Domain;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Parsing
{
    public class EndsWithMatcher : BaseMatcher, IMatcher
    {
        private HashSet<string> itemsToCompare = new HashSet<string>();

        public EndsWithMatcher(List<string> compareTo)
        {
            if (compareTo != null)
            {
                itemsToCompare.UnionWith(compareTo);
            }
        }

        public override bool Match(string key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            if (String.IsNullOrEmpty(key))
            {
                return false;
            }

            return itemsToCompare.Any(i => key.EndsWith(i));
        }

        public override bool Match(Key key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return Match(key.matchingKey, attributes, splitClient);
        }

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }
    }
}