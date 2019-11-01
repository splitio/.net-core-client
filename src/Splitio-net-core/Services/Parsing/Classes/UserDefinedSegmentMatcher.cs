using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Evaluator;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public class UserDefinedSegmentMatcher : BaseMatcher
    {
        private string segmentName;
        private ISegmentCache segmentsCache;

        public UserDefinedSegmentMatcher(string segmentName, ISegmentCache segmentsCache)
        {
            this.segmentName = segmentName;
            this.segmentsCache = segmentsCache;
        }

        public override bool Match(string key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return segmentsCache.IsInSegment(segmentName, key);
        }

        public override bool Match(Key key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return Match(key.matchingKey, attributes, evaluator);
        }

        public override bool Match(DateTime key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }

        public override bool Match(long key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }

        public override bool Match(List<string> key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }

        public override bool Match(bool key, Dictionary<string, object> attributes = null, IEvaluator evaluator = null)
        {
            return false;
        }
    }
}
