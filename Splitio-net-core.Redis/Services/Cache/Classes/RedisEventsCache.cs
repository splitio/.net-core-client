using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Shared.Interfaces;

namespace Splitio.Redis.Services.Cache.Classes
{
    public class RedisEventsCache : RedisCacheBase, ISimpleCache<Event>
    {
        private const string eventKeyPrefix = "events";

        public RedisEventsCache(IRedisAdapter redisAdapter, string machineIP, string sdkVersion, string userPrefix = null)
            : base(redisAdapter, machineIP, sdkVersion, userPrefix) 
        {}

        public void AddItem(Event item)
        {
            var key = redisKeyPrefix + eventKeyPrefix;
            var eventJson = JsonConvert.SerializeObject(item);
            redisAdapter.ListRightPush(key, eventJson);
        }
    }
}
