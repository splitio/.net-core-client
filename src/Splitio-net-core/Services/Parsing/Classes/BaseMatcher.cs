using Splitio.Domain;
using Splitio.Services.Evaluator;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing.Classes
{
    public abstract class BaseMatcher : IMatcher
    {
        public abstract bool Match(string key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public abstract bool Match(DateTime key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public abstract bool Match(long key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public abstract bool Match(List<string> key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public abstract bool Match(Key key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public abstract bool Match(bool key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public bool Match(object value, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            if (value is bool)
            {
                return Match((bool)value, attributes, evaluator);
            }
            else if (value is string)
            {
                return Match((string)value, attributes, evaluator);
            }
            else if (value is DateTime)
            {
                return Match((DateTime)value, attributes, evaluator);
            }
            else if (value is long)
            {
                return Match((long)value, attributes, evaluator);
            }
            else if (value is int)
            {
                return Match((int)value, attributes, evaluator);
            }
            else if (value is List<string>)
            {
                return Match((List<string>)value, attributes, evaluator);
            }
            else if (value is Key)
            {
                return Match((Key)value, attributes, evaluator);
            }

            return false;
        }
    }
}