using Splitio.Services.Cache.Interfaces;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Cache.Classes
{
    public class RedisSegmentCache : RedisCacheBase, ISegmentCache
    {
        private const string segmentKeyPrefix = "segment.";
        private const string segmentNameKeyPrefix = "segment.{segmentname}.";
        private const string segmentsKeyPrefix = "segments.";

        public RedisSegmentCache(IRedisAdapter redisAdapter, string userPrefix = null) : base(redisAdapter, userPrefix) { }

        public void AddToSegment(string segmentName, List<string> segmentKeys)
        {
            var key = redisKeyPrefix + segmentKeyPrefix + segmentName;
            var valuesToAdd = segmentKeys.Select(x => (RedisValue)x).ToArray();
            redisAdapter.SAdd(key, valuesToAdd);
        }

        public void RemoveFromSegment(string segmentName, List<string> segmentKeys)
        {
            var key = redisKeyPrefix + segmentKeyPrefix + segmentName;
            var valuesToRemove = segmentKeys.Select(x => (RedisValue)x).ToArray();
            redisAdapter.SRem(key, valuesToRemove);
        }

        public bool IsInSegment(string segmentName, string key)
        {
            var redisKey = redisKeyPrefix + segmentKeyPrefix + segmentName;
            return redisAdapter.SIsMember(redisKey, key);
        }

        public void SetChangeNumber(string segmentName, long changeNumber)
        {
            var key = redisKeyPrefix + segmentNameKeyPrefix.Replace("{segmentname}", segmentName) + "till";
            redisAdapter.Set(key, changeNumber.ToString());
        }

        public long GetChangeNumber(string segmentName)
        {
            var key = redisKeyPrefix + segmentNameKeyPrefix.Replace("{segmentname}", segmentName) + "till";
            string changeNumberString = redisAdapter.Get(key);
            long changeNumberParsed;
            var result = long.TryParse(changeNumberString, out changeNumberParsed);

            return result ? changeNumberParsed : -1;
        }

        public long RegisterSegment(string segmentName)
        {
            return RegisterSegments(new List<string>() { segmentName });
        }

        public long RegisterSegments(List<string> segmentNames)
        {
            var key = redisKeyPrefix + segmentsKeyPrefix + "registered";
            var segments = segmentNames.Select(x => (RedisValue)x).ToArray();
            return redisAdapter.SAdd(key, segments);
        }

        public List<string> GetRegisteredSegments()
        {
            var key = redisKeyPrefix + segmentsKeyPrefix + "registered";
            var result = redisAdapter.SMembers(key);

            return result.Select(x => (string)x).ToList();
        }

        public void Flush()
        {
            redisAdapter.Flush();
        }

        public void Clear()
        {
            return;
        }
    }
}
