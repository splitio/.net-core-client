using Splitio.Domain;

namespace Splitio.Redis.Services.Domain
{
    public class RedisConfig : BaseConfig
    {
        public string RedisHost { get; set; }
        public string RedisPort { get; set; }
        public string RedisPassword { get; set; }
        public string RedisUserPrefix { get; set; }
        public int RedisDatabase { get; set; }
        public int RedisConnectTimeout { get; set; }
        public int RedisConnectRetry { get; set; }
        public int RedisSyncTimeout { get; set; }
    }
}
