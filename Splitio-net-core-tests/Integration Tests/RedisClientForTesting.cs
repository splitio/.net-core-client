using Common.Logging;
using Splitio.Redis.Services.Client.Classes;
using Splitio.Services.Client.Classes;

namespace Splitio_Tests.Integration_Tests
{
    public class RedisClientForTesting : RedisClient
    {
        public RedisClientForTesting(ConfigurationOptions config, ILog log) 
            : base(config, log)
        {
            Ready = false;
        }
    }
}
