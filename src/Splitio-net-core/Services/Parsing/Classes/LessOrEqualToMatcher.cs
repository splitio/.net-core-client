using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Client.Interfaces;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public class LessOrEqualToMatcher : CompareMatcher, IMatcher
    {
        public LessOrEqualToMatcher(DataTypeEnum? dataType, long value)
        {
            this.dataType = dataType;
            this.value = value;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return key <= value;
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            var date = value.ToDateTime();
            key = key.Truncate(TimeSpan.FromMinutes(1)); // Truncate to whole minute
            return key <= date;
        }
    }
}
