using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Splitio.Services.Parsing.Classes
{
    public class MatchesStringMatcher : BaseMatcher, IMatcher
    {
        string value;
        Regex regex;


        public MatchesStringMatcher(string pattern)
        {
            regex = new Regex(pattern);
        }


        public override bool Match(string key, Dictionary<string, object> attributes = null, Client.Interfaces.ISplitClient splitClient = null)
        {
            return regex.IsMatch(key);
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

        public override bool Match(bool key, Dictionary<string, object> attributes = null, Client.Interfaces.ISplitClient splitClient = null)
        {
            return false;
        }
    }
}
