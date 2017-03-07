using Splitio.Services.Cache.Interfaces;
using System;

namespace Splitio.Services.Parsing
{
    public class UserDefinedSegmentMatcher: IMatcher
    {
        private string segmentName;
        private ISegmentCache segmentsCache;

        public UserDefinedSegmentMatcher(string segmentName, ISegmentCache segmentsCache)
        {
            this.segmentName = segmentName;
            this.segmentsCache = segmentsCache;
        }

        public bool Match(string key)
        {
            return segmentsCache.IsInSegment(segmentName, key);
        }

        public bool Match(DateTime key)
        {
            return false;
        }

        public bool Match(long key)
        {
            return false;
        }
    }
}
