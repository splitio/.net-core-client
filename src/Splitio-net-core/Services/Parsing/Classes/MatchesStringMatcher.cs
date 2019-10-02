using Splitio.Domain;
using Splitio.Services.Evaluator;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Splitio.Services.Parsing.Classes
{
    public class MatchesStringMatcher : BaseMatcher
    {
        Regex regex;

        public MatchesStringMatcher(string pattern)
        {
            regex = new Regex(pattern);
        }

        public override bool Match(string key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return regex.IsMatch(key);
        }

        public override bool Match(Key key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return regex.IsMatch(key.matchingKey);
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }

        public override bool Match(bool key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }
    }
}
