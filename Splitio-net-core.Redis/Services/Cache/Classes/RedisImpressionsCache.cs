using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Shared.Interfaces;
using System;
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
    }
}
