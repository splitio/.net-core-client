using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Linq;
using System.Threading;

namespace Splitio.Services.Events.Classes
{
    public class SelfUpdatingEventLog : IListener<WrappedEvent>
    {
        public static long MAX_SIZE_BYTES = 5 * 1024 * 1024L;

        private readonly IWrapperAdapter _wrapperAdapter;

        private long AcumulateSize;
        private int interval;
        private int firstPushWindow;
        private IEventSdkApiClient apiClient;
        private ISimpleProducerCache<WrappedEvent> wrappedEventsCache;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected static readonly ILog Logger = LogManager.GetLogger(typeof(SelfUpdatingEventLog));

        public SelfUpdatingEventLog(IEventSdkApiClient apiClient, int firstPushWindow, int interval, ISimpleCache<WrappedEvent> eventsCache, int maximumNumberOfKeysToCache = -1)
        {
            this.wrappedEventsCache = (eventsCache as ISimpleProducerCache<WrappedEvent>) ?? new InMemorySimpleCache<WrappedEvent>(new BlockingQueue<WrappedEvent>(maximumNumberOfKeysToCache));
            this.apiClient = apiClient;
            this.interval = interval;
            this.firstPushWindow = firstPushWindow;

            _wrapperAdapter = new WrapperAdapter();
        }

        public void Start()
        {
            _wrapperAdapter
                .TaskDelay(firstPushWindow * 1000)
                .ContinueWith((t) => {
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
            if (wrappedEventsCache.HasReachedMaxSize())
            {
                Logger.Warn("Split SDK events queue is full. Events may have been dropped. Consider increasing capacity.");
            }

            var wrappedEvents = wrappedEventsCache.FetchAllAndClear();

            if (wrappedEvents.Count > 0)
            {
                try
                {
                    var events = wrappedEvents
                        .Select(x => x.Event)
                        .ToList();

                    apiClient.SendBulkEvents(events);

                    AcumulateSize = 0;
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught updating events.", e);
                }
            }
        }

        public void Log(WrappedEvent item)
        {
            wrappedEventsCache.AddItem(item);

            AcumulateSize += item.Size;

            if (wrappedEventsCache.HasReachedMaxSize() || AcumulateSize >= MAX_SIZE_BYTES)
            {
                SendBulkEvents();
            }
        }
    }
}