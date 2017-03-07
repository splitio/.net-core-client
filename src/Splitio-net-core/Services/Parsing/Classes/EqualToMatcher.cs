using Splitio.CommonLibraries;
using Splitio.Domain;
using System;

namespace Splitio.Services.Parsing
{
    public class EqualToMatcher : CompareMatcher, IMatcher
    {
        public EqualToMatcher(DataTypeEnum? dataType, long value)
        {
            this.dataType = dataType;
            this.value = value;
        }

        public override bool Match(long key)
        {
            return value == key;
        }

        public override bool Match(DateTime key)
        {
            var date = value.ToDateTime();

            return date.Date == key.Date; // Compare just date part
        }
    }
}
