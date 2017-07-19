using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
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

        public override bool Match(string key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return segmentsCache.IsInSegment(segmentName, key);
        }

        public override bool Match(Key key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return Match(key.matchingKey, attributes, splitClient);
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }

        public override bool Match(bool key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            return false;
        }
    }
}
