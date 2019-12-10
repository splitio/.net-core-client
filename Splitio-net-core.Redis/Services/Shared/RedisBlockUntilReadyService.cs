using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Shared.Interfaces;
using System;

namespace Splitio.Redis.Services.Shared
{
    public class RedisBlockUntilReadyService : IBlockUntilReadyService
    {
        private readonly IRedisAdapter _redisAdapter;

        public RedisBlockUntilReadyService(IRedisAdapter redisAdapter)
        {
            _redisAdapter = redisAdapter;
        }

        public void BlockUntilReady(int blockMilisecondsUntilReady = 0)
        {
            if (!IsSdkReady())
            {
                throw new TimeoutException($"SDK was not ready. Could not connect to Redis");
            }
        }

        public bool IsSdkReady()
        {
            return _redisAdapter.IsConnected();
        }
    }
}
