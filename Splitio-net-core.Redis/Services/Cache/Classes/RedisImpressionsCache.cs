using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Redis.Services.Cache.Classes
{
    public class RedisImpressionsCache : RedisCacheBase, ISimpleCache<KeyImpression>
    {
        private const string impressionKeyPrefix = "impressions.";

        public RedisImpressionsCache(IRedisAdapter redisAdapter, string machineIP, string sdkVersion, string userPrefix = null)
            : base(redisAdapter, machineIP, sdkVersion, userPrefix) 
        {}

        public void AddItem(KeyImpression item)
        {
            var key = redisKeyPrefix + impressionKeyPrefix + item.feature;
            var impressionJson = JsonConvert.SerializeObject(item);
            redisAdapter.SAdd(key, impressionJson);
        }

        public List<KeyImpression> FetchAllAndClear()
        {
            var impressions = new List<KeyImpression>();
            var pattern = redisKeyPrefix + impressionKeyPrefix + "*";
            var impresionKeys = redisAdapter.Keys(pattern);
            foreach(var impresionKey in impresionKeys)
            {
                var impressionsJson = redisAdapter.SMembers(impresionKey);
                var result = impressionsJson.Select(x => JsonConvert.DeserializeObject<KeyImpression>(x)).ToList();
                foreach (var impression in result)
                {
                    impression.feature = impresionKey.ToString().Replace(redisKeyPrefix + impressionKeyPrefix, "");
                    impressions.Add(impression);
                }
                redisAdapter.Del(impresionKey);
            }
            return impressions;
        }

        public bool HasReachedMaxSize()
        {
            return false;
        }
    }
}
