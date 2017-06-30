using Splitio.Services.Client.Interfaces;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using Splitio.Domain;

namespace Splitio.Services.Parsing
{
    public class ContainsAnyOfSetMatcher : BaseMatcher, IMatcher
    {
        private HashSet<string> itemsToCompare = new HashSet<string>();

        public ContainsAnyOfSetMatcher(List<string> compareTo)
        {
            if (compareTo != null)
            {
                itemsToCompare.UnionWith(compareTo);
            }
        }

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            if (key == null || itemsToCompare.Count == 0)
            {
                return false;
            }

            return itemsToCompare.Any(i => key.Contains(i));
        }

        public override bool Match(string key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
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

        public override bool Match(Key key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }
    }
}