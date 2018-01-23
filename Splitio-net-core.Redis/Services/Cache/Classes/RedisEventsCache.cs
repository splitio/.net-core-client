using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Redis.Services.Cache.Classes
{
    public class RedisEventsCache : RedisCacheBase, ISimpleCache<Event>
    {
        private const string eventKeyPrefix = "events.";

        public RedisEventsCache(IRedisAdapter redisAdapter, string machineIP, string sdkVersion, string userPrefix = null)
            : base(redisAdapter, machineIP, sdkVersion, userPrefix) 
        {}

        public void AddItem(Event item)
        {
            var key = redisKeyPrefix + eventKeyPrefix + Guid.NewGuid();
            var eventJson = JsonConvert.SerializeObject(item);
            redisAdapter.SAdd(key, eventJson);
        }

        public List<Event> FetchAllAndClear()
        {
            var events = new List<Event>();
            var pattern = redisKeyPrefix + eventKeyPrefix + "*";
            var eventKeys = redisAdapter.Keys(pattern);
            foreach(var eventKey in eventKeys)
            {
                var eventJson = redisAdapter.SMembers(eventKey);
                var result = eventJson.Select(x => JsonConvert.DeserializeObject<Event>(x)).ToList();
                events.AddRange(result);
                redisAdapter.Del(eventKey);
            }
            return events;
        }

        public bool HasReachedMaxSize()
        {
            return false;
        }
    }
}
