using Splitio.Services.Client.Interfaces;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using Splitio.Domain;

namespace Splitio.Services.Parsing
{
    public class AllKeysMatcher : BaseMatcher, IMatcher
    {
        public override bool Match(string key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return key != null;
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return key != null;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return key != null;
        }

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(Key key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return key.matchingKey != null;
        }
    }
}
