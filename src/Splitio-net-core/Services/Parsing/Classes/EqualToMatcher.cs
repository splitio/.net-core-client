using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Evaluator;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public class EqualToMatcher : CompareMatcher
    {
        public EqualToMatcher(DataTypeEnum? dataType, long value)
        {
            this.dataType = dataType;
            this.value = value;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            if (dataType == DataTypeEnum.DATETIME)
            {
                return Match(key.ToDateTime(), attributes, evaluator);
            }

            return value == key;
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            var date = value.ToDateTime();

            return date.ToUniversalTime().Date == key.ToUniversalTime().Date; // Compare just date part
        }

        public override bool Match(bool key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }
    }
}
