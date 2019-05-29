using Splitio.Domain;
using Splitio.Services.Shared.Interfaces;

namespace Splitio.Redis.Services.Events.Classes
{
    public class RedisEventLog : IListener<WrappedEvent>
    {
        private ISimpleCache<WrappedEvent> eventsCache;

        public RedisEventLog(ISimpleCache<WrappedEvent> eventsCache)
        {
            this.eventsCache = eventsCache;
        }

        public void Log(WrappedEvent item)
        {
            eventsCache.AddItem(item);
        }
    }
}