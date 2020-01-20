using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio.Services.Events.Classes
{
    public class EventsLog : IEventsLog
    {
        private static readonly long MAX_SIZE_BYTES = 5 * 1024 * 1024L;
        protected static readonly ISplitLogger Logger = WrapperAdapter.GetLogger(typeof(EventsLog));

        private readonly IWrapperAdapter _wrapperAdapter;
        private readonly IEventSdkApiClient _apiClient;
        private readonly ISimpleProducerCache<WrappedEvent> _wrappedEventsCache;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _interval;
        private readonly int _firstPushWindow;
        
        private long _acumulateSize;
        
        public EventsLog(IEventSdkApiClient apiClient, 
            int firstPushWindow, 
            int interval, 
            ISimpleCache<WrappedEvent> eventsCache, 
            int maximumNumberOfKeysToCache = -1)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _wrappedEventsCache = (eventsCache as ISimpleProducerCache<WrappedEvent>) ?? new InMemorySimpleCache<WrappedEvent>(new BlockingQueue<WrappedEvent>(maximumNumberOfKeysToCache));
            _apiClient = apiClient;
            _interval = interval;
            _firstPushWindow = firstPushWindow;

            _wrapperAdapter = new WrapperAdapter();
        }

        public void Start()
        {
            _wrapperAdapter
                .TaskDelay(_firstPushWindow * 1000)
                .ContinueWith((t) => {
                    SendBulkEvents();
                    PeriodicTaskFactory.Start(() => { SendBulkEvents(); }, _interval * 1000, _cancellationTokenSource.Token);
                });
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            SendBulkEvents();
        }

        public void Log(WrappedEvent wrappedEvent)
        {
            _wrappedEventsCache.AddItems(new List<WrappedEvent> { wrappedEvent });

            _acumulateSize += wrappedEvent.Size;

            if (_wrappedEventsCache.HasReachedMaxSize() || _acumulateSize >= MAX_SIZE_BYTES)
            {
                SendBulkEvents();
            }
        }

        private void SendBulkEvents()
        {
            if (_wrappedEventsCache.HasReachedMaxSize())
            {
                Logger.Warn("Split SDK events queue is full. Events may have been dropped. Consider increasing capacity.");
            }

            var wrappedEvents = _wrappedEventsCache.FetchAllAndClear();

            if (wrappedEvents.Count > 0)
            {
                try
                {
                    var events = wrappedEvents
                        .Select(x => x.Event)
                        .ToList();

                    _apiClient.SendBulkEvents(events);

                    _acumulateSize = 0;
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught updating events.", e);
                }
            }
        }
    }
}