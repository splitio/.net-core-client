using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Cache.Classes
{
    public class RedisImpressionsCache : RedisCacheBase, IImpressionsCache
    {
        private const string impressionKeyPrefix = "impressions.";

        public RedisImpressionsCache(IRedisAdapter redisAdapter, string machineIP, string sdkVersion, string userPrefix = null)
            : base(redisAdapter, machineIP, sdkVersion, userPrefix) 
        {}

        public void AddImpression(KeyImpression impression)
        {
            var key = redisKeyPrefix + impressionKeyPrefix + impression.feature;
            var impressionJson = JsonConvert.SerializeObject(impression);
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
    }
}
