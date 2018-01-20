using Common.Logging;
using Newtonsoft.Json;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.Events.Classes
{
    public class SelfUpdatingEventLog : IListener<Event>
    {
        private IEventSdkApiClient apiClient;
        private int interval;
        private int firstPushWindow;
        private ISimpleCache<Event> eventsCache;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected static readonly ILog Logger = LogManager.GetLogger(typeof(SelfUpdatingEventLog));

        public SelfUpdatingEventLog(IEventSdkApiClient apiClient, int firstPushWindow, int interval, ISimpleCache<Event> eventsCache, int maximumNumberOfKeysToCache = -1)
        {
            this.eventsCache = eventsCache ?? new InMemorySimpleCache<Event>(new BlockingQueue<Event>(maximumNumberOfKeysToCache));
            this.apiClient = apiClient;
            this.interval = interval;
            this.firstPushWindow = firstPushWindow;
        }

        public void Start()
        {
            Task.Delay(firstPushWindow * 1000).ContinueWith((t) => {
                SendBulkEvents();
                PeriodicTaskFactory.Start(() => { SendBulkEvents(); }, interval * 1000, cancellationTokenSource.Token);
            });
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            SendBulkEvents();
        }


        private void SendBulkEvents()
        {
            if (eventsCache.HasReachedMaxSize())
            {
                Logger.Warn("Split SDK events queue is full. Events may have been dropped. Consider increasing capacity.");
            }

            var events = eventsCache.FetchAllAndClear();

            if (events.Count > 0)
            {
                try
                {
                    var eventsJson = JsonConvert.SerializeObject(events, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    apiClient.SendBulkEvents(eventsJson);
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught updating events.", e);
                }
            }
        }

        public void Log(Event item)
        {
            eventsCache.AddItem(item);
        }
    }
}