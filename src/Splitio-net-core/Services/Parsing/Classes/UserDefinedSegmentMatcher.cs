using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public class UserDefinedSegmentMatcher: BaseMatcher, IMatcher
    {
        private string segmentName;
        private ISegmentCache segmentsCache;

        public UserDefinedSegmentMatcher(string segmentName, ISegmentCache segmentsCache)
        {
            this.segmentName = segmentName;
            this.segmentsCache = segmentsCache;
        }

        public override bool Match(string key)
        {
            return segmentsCache.IsInSegment(segmentName, key);
        }

        public override bool Match(DateTime key)
        {
            return false;
        }

        public override bool Match(long key)
        {
            return false;
        }

        public override bool Match(List<string> key)
        {
            return false;
        }
    }
}
