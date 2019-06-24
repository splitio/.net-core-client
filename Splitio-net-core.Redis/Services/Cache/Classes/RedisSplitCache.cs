using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Cache.Interfaces;
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
            throw new System.NotImplementedException();
        }

        public bool RemoveSplit(string splitName)
        {
            throw new System.NotImplementedException();
        }

        public void SetChangeNumber(long changeNumber)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public void Flush()
        {
            throw new System.NotImplementedException();
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
    }
}
 