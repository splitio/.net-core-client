using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Cache.Interfaces;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Redis.Services.Cache.Classes
{
    public class RedisSplitCache : RedisCacheBase, ISplitCache
    {
        private const string splitKeyPrefix = "split.";
        private const string splitsKeyPrefix = "splits.";

        public RedisSplitCache(IRedisAdapter redisAdapter, string userPrefix = null) 
            : base(redisAdapter, userPrefix)
        { }
        
        public void AddSplit(string splitName, SplitBase split)
        {
            var key = redisKeyPrefix + splitKeyPrefix + splitName;
            var splitJson = JsonConvert.SerializeObject(split);

            redisAdapter.Set(key, splitJson);

            AddTrafficType(split);
        }

        public bool RemoveSplit(string splitName)
        {
            var key = redisKeyPrefix + splitKeyPrefix + splitName;

            var split = GetSplit(splitName);
            var removed = redisAdapter.Del(key);

            RemoveTrafficType(removed, split);

            return removed;
        }

        public void SetChangeNumber(long changeNumber)
        {
            var key = redisKeyPrefix + splitsKeyPrefix + "till";
            redisAdapter.Set(key, changeNumber.ToString());
        }

        public long GetChangeNumber()
        {
            var key = redisKeyPrefix + splitsKeyPrefix + "till";
            var changeNumberString = redisAdapter.Get(key);
            var result = long.TryParse(changeNumberString, out long changeNumberParsed);
            
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
                var splits = splitValues.Where(x=>!x.IsNull).Select(x => JsonConvert.DeserializeObject<Split>(x));

                return splits.Cast<SplitBase>().ToList();
            }
            
            return new List<SplitBase>();          
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

        public void Clear()
        {
            return;
        }

        public bool TrafficTypeExists(string trafficType)
        {
            if (string.IsNullOrEmpty(trafficType)) return false;

            var value = redisAdapter.Get(GetTrafficTypeKey(trafficType));

            var quantity = value ?? "0";

            int.TryParse(quantity, out int quantityInt);

            return quantityInt > 0;
        }

        private string GetTrafficTypeKey(string type)
        {
            return $"{redisKeyPrefix}trafficType.{type}";
        }

        private void AddTrafficType(SplitBase splitBase)
        {
            if (splitBase == null) return;

            var split = (Split)splitBase;

            if (string.IsNullOrEmpty(split.trafficTypeName)) return;

            var ttKey = GetTrafficTypeKey(split.trafficTypeName);
            
            var value = redisAdapter.Get(ttKey);

            if (string.IsNullOrEmpty(value)) value = "0";

            int.TryParse(value, out int valueInt);

            redisAdapter.Set(ttKey, (valueInt++).ToString());
        }

        private void RemoveTrafficType(bool removed, SplitBase splitBase)
        {
            if (!removed || splitBase == null) return;

            var split = (Split)splitBase;

            if (string.IsNullOrEmpty(split.trafficTypeName)) return;

            var ttKey = GetTrafficTypeKey(split.trafficTypeName);
            var value = redisAdapter.Get(ttKey);

            int.TryParse(value, out int valueInt);

            if (valueInt <= 0)
            {
                redisAdapter.Del(ttKey);
            }
            else
            {
                redisAdapter.Set(ttKey, (valueInt--).ToString());
            }
        }
    }
}
 