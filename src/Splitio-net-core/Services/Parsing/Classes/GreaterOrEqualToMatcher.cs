using Splitio.CommonLibraries;
using Splitio.Domain;
using System;

namespace Splitio.Services.Parsing
{
    public class GreaterOrEqualToMatcher: CompareMatcher, IMatcher
    {
        public GreaterOrEqualToMatcher(DataTypeEnum? dataType, long value)
        {
            this.dataType = dataType;
            this.value = value;
        }

        public override bool Match(long key)
        {
            return key >= value;
        }

        public override bool Match(DateTime key)
        {
            var date = value.ToDateTime();
            key = key.Truncate(TimeSpan.FromMinutes(1)); // Truncate to whole minute
            return key >= date;
        }
    }
}
