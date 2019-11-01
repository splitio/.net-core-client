using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Cache.Interfaces;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Redis.Services.Cache.Classes
{
    public class RedisSegmentCache : RedisCacheBase, ISegmentCache
    {
        private const string segmentKeyPrefix = "segment.";
        private const string segmentNameKeyPrefix = "segment.{segmentname}.";
        private const string segmentsKeyPrefix = "segments.";

        public RedisSegmentCache(IRedisAdapter redisAdapter, 
            string userPrefix = null) : base(redisAdapter, userPrefix)
        { }

        public void AddToSegment(string segmentName, List<string> segmentKeys)
        {
            var key = $"{RedisKeyPrefix}{segmentKeyPrefix}{segmentName}";
            var valuesToAdd = segmentKeys.Select(x => (RedisValue)x).ToArray();

            _redisAdapter.SAdd(key, valuesToAdd);
        }

        public void RemoveFromSegment(string segmentName, List<string> segmentKeys)
        {
            var key = $"{RedisKeyPrefix}{segmentKeyPrefix}{segmentName}";
            var valuesToRemove = segmentKeys.Select(x => (RedisValue)x).ToArray();

            _redisAdapter.SRem(key, valuesToRemove);
        }

        public bool IsInSegment(string segmentName, string key)
        {
            var redisKey = $"{RedisKeyPrefix}{segmentKeyPrefix}{segmentName}";

            return _redisAdapter.SIsMember(redisKey, key);
        }

        public void SetChangeNumber(string segmentName, long changeNumber)
        {
            var key = RedisKeyPrefix + segmentNameKeyPrefix.Replace("{segmentname}", segmentName) + "till";

            _redisAdapter.Set(key, changeNumber.ToString());
        }

        public long GetChangeNumber(string segmentName)
        {
            var key = RedisKeyPrefix + segmentNameKeyPrefix.Replace("{segmentname}", segmentName) + "till";
            var changeNumberString = _redisAdapter.Get(key);
            var result = long.TryParse(changeNumberString, out long changeNumberParsed);

            return result ? changeNumberParsed : -1;
        }

        public long RegisterSegment(string segmentName)
        {
            return RegisterSegments(new List<string>() { segmentName });
        }

        public long RegisterSegments(List<string> segmentNames)
        {
            var key = $"{RedisKeyPrefix}{segmentsKeyPrefix}registered";
            var segments = segmentNames.Select(x => (RedisValue)x).ToArray();

            return _redisAdapter.SAdd(key, segments);
        }

        public List<string> GetRegisteredSegments()
        {
            var key = $"{RedisKeyPrefix}{segmentsKeyPrefix}registered";
            var result = _redisAdapter.SMembers(key);

            return result.Select(x => (string)x).ToList();
        }

        public void Flush()
        {
            _redisAdapter.Flush();
        }

        public void Clear()
        {
            return;
        }
    }
}
