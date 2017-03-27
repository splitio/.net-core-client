using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Cache.Classes
{
    public class RedisSplitCache : RedisCacheBase, ISplitCache
    {
        private const string splitKeyPrefix = "split.";
        private const string splitsKeyPrefix = "splits.";

        public RedisSplitCache(IRedisAdapter redisAdapter, string userPrefix = null) : base(redisAdapter, userPrefix) { }
        
        public void AddSplit(string splitName, SplitBase split)
        {
            var key = redisKeyPrefix + splitKeyPrefix + splitName;
            var splitJson = JsonConvert.SerializeObject(split);
            redisAdapter.Set(key, splitJson);
        }

        public bool RemoveSplit(string splitName)
        {
            var key = redisKeyPrefix + splitKeyPrefix + splitName;
            return redisAdapter.Del(key);
        }

        public void SetChangeNumber(long changeNumber)
        {
            var key = redisKeyPrefix + splitsKeyPrefix + "till";
            redisAdapter.Set(key, changeNumber.ToString());
        }

        public long GetChangeNumber()
        {
            var key = redisKeyPrefix + splitsKeyPrefix + "till";
            string changeNumberString = redisAdapter.Get(key);
            long changeNumberParsed;
            var result = long.TryParse(changeNumberString, out changeNumberParsed);
            
            return result ? changeNumberParsed : -1;
        }

        public SplitBase GetSplit(string splitName)
        {
            var key = redisKeyPrefix + splitKeyPrefix + splitName;
            var splitJson = redisAdapter.Get(key);
            return !string.IsNullOrEmpty(splitJson) ? JsonConvert.DeserializeObject<Split>(splitJson) : null;
        }

        public List<SplitBase> GetAllSplits()
        {
            var pattern = redisKeyPrefix + splitKeyPrefix + "*";
            var splitKeys = redisAdapter.Keys(pattern);
            var splitValues = redisAdapter.Get(splitKeys);
            if (splitValues != null && splitValues.Count()>0)
            {
                var splits = splitValues.Select(x => JsonConvert.DeserializeObject<Split>(x));
                return splits.Cast<SplitBase>().ToList();
            }
            else
            {
                return new List<SplitBase>();
            }          
        }

        public List<string> GetKeys()
        {
            var pattern = redisKeyPrefix + splitKeyPrefix + "*";
            var splitKeys = redisAdapter.Keys(pattern);
            var result = splitKeys.Select(x => x.ToString()).ToList();

            return result;
        }

        public long RemoveSplits(List<string> splitNames)
        {
            var keys = splitNames.Select(x => (RedisKey)(redisKeyPrefix + splitKeyPrefix + x)).ToArray();
            return redisAdapter.Del(keys);
        }


        public void Flush()
        {
            redisAdapter.Flush();
        }
    }
}
