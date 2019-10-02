using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Evaluator;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public abstract class CompareMatcher : BaseMatcher
    {
        protected DataTypeEnum? dataType;
        protected long value;
        protected long start;
        protected long end;

        public override bool Match(string key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            switch (dataType)
            {
                case DataTypeEnum.DATETIME:
                    var date = key.ToDateTime();
                    return date != null ? Match(date.Value) : false;
                case DataTypeEnum.NUMBER:
                    long number;
                    var result = long.TryParse(key, out number);
                    return result ? Match(number) : false;
                default: return false;
            }
        }

        public abstract override bool Match(DateTime key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public abstract override bool Match(long key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null);

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }

        public override bool Match(Key key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return Match(key.matchingKey, attributes, evaluator);
        }
        
         public override bool Match(bool key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }
    }
}
