using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Client.Interfaces;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public class BetweenMatcher : CompareMatcher, IMatcher
    {

        public BetweenMatcher(DataTypeEnum? dataType, long start, long end)
        {
            this.dataType = dataType;
            this.start = start;
            this.end = end;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return (start <= key) && (key <= end);         
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            var startDate = start.ToDateTime();
            var endDate = end.ToDateTime();
            key = key.Truncate(TimeSpan.FromMinutes(1)); // Truncate to whole minute
            return (startDate.ToUniversalTime() <= key.ToUniversalTime()) && (key.ToUniversalTime() <= endDate.ToUniversalTime());
        }
    }
}
