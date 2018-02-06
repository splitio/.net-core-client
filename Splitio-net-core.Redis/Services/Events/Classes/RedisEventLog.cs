using Splitio.Domain;
using Splitio.Services.Shared.Interfaces;

namespace Splitio.Redis.Services.Events.Classes
{
    public class RedisEventLog : IListener<Event>
    {
        private ISimpleCache<Event> eventsCache;

        public RedisEventLog(ISimpleCache<Event> eventsCache)
        {
            this.eventsCache = eventsCache;
        }

        public void Log(Event item)
        {
            eventsCache.AddItem(item);
        }
    }
}