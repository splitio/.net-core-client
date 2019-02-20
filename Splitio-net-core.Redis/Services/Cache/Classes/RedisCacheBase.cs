using Splitio.Redis.Services.Cache.Interfaces;

namespace Splitio.Redis.Services.Cache.Classes
{
    public abstract class RedisCacheBase
    {
        private const string RedisKeyPrefixFormat = "SPLITIO/{sdk-language-version}/{instance-id}/";
        protected IRedisAdapter redisAdapter;
        protected string redisKeyPrefix;

        protected string UserPrefix;
        protected string SdkVersion;
        protected string MachineIp;

        public RedisCacheBase(IRedisAdapter redisAdapter, string userPrefix = null)
        {
            UserPrefix = userPrefix;
            this.redisAdapter = redisAdapter;
            redisKeyPrefix = "SPLITIO.";

            if (!string.IsNullOrEmpty(userPrefix))
            {
                redisKeyPrefix = userPrefix + "." + redisKeyPrefix;
            }
        }

        public RedisCacheBase(IRedisAdapter redisAdapter, string machineIP, string sdkVersion, string userPrefix = null)
        {
            UserPrefix = userPrefix;
            MachineIp = machineIP;
            SdkVersion = sdkVersion;

            this.redisAdapter = redisAdapter;
            redisKeyPrefix = RedisKeyPrefixFormat
                .Replace("{sdk-language-version}", sdkVersion)
                .Replace("{instance-id}", machineIP);

            if (!string.IsNullOrEmpty(userPrefix))
            {
                redisKeyPrefix = userPrefix + "." + redisKeyPrefix;
            }
        }
    }
}
