using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing.Classes
{
    public class EqualToBooleanMatcher : BaseMatcher, IMatcher
    {
        bool value;

        public EqualToBooleanMatcher(bool value)
        {
            this.value = value;
        }

        public override bool Match(bool key, Dictionary<string, object> attributes = null, Client.Interfaces.ISplitClient splitClient = null)
        {
            return key.Equals(value);
        }

        public override bool Match(string key, Dictionary<string, object> attributes = null, Client.Interfaces.ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, Client.Interfaces.ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, Client.Interfaces.ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, Client.Interfaces.ISplitClient splitClient = null)
        {
            return false;
        }
    }
}
