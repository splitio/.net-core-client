using Splitio.Redis.Services.Cache.Interfaces;

namespace Splitio.Redis.Services.Cache.Classes
{
    public abstract class RedisCacheBase
    {
        private const string RedisKeyPrefixFormat = "SPLITIO/{sdk-language-version}/{instance-id}/";
        protected IRedisAdapter redisAdapter;
        protected string redisKeyPrefix;

        public RedisCacheBase(IRedisAdapter redisAdapter, string userPrefix = null)
        {
            this.redisAdapter = redisAdapter;
            this.redisKeyPrefix = "SPLITIO.";

            if (!string.IsNullOrEmpty(userPrefix))
            {
                this.redisKeyPrefix = userPrefix + "." + redisKeyPrefix;
            }
        }

        public RedisCacheBase(IRedisAdapter redisAdapter, string machineIP, string sdkVersion, string userPrefix = null)
        {
            this.redisAdapter = redisAdapter;
            this.redisKeyPrefix = RedisKeyPrefixFormat.Replace("{sdk-language-version}", sdkVersion)
                                                          .Replace("{instance-id}", machineIP);
            if (!string.IsNullOrEmpty(userPrefix))
            {
                this.redisKeyPrefix = userPrefix + "." + redisKeyPrefix;
            }
        }
    }
}
