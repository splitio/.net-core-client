using Splitio.Domain;
using Splitio.Services.Evaluator;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public interface IMatcher
    {
        bool Match(object value, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        bool Match(string key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        bool Match(Key key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        bool Match(DateTime key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        bool Match(long key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        bool Match(List<string> key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        bool Match(bool key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);
    }
}
