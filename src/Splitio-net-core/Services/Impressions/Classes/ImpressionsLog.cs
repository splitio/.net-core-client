using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionsLog : IImpressionsLog
    {
        private readonly ITreatmentSdkApiClient _apiClient;
        private readonly ISimpleProducerCache<KeyImpression> _impressionsCache;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _interval;

        protected static readonly ISplitLogger Logger = WrapperAdapter.GetLogger(typeof(ImpressionsLog));

        public ImpressionsLog(ITreatmentSdkApiClient apiClient,
            int interval,
            ISimpleCache<KeyImpression> impressionsCache,
            int maximumNumberOfKeysToCache = -1)
        {
            _apiClient = apiClient;
            _impressionsCache = (impressionsCache as ISimpleProducerCache<KeyImpression>) ?? new InMemorySimpleCache<KeyImpression>(new BlockingQueue<KeyImpression>(maximumNumberOfKeysToCache));            
            _interval = interval;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            PeriodicTaskFactory.Start(() => { SendBulkImpressions(); }, _interval * 1000, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            SendBulkImpressions();
        }

        public void AddItems(IList<KeyImpression> impressions)
        {
            _impressionsCache.AddItems(impressions);
        }

        private void SendBulkImpressions()
        {
            if (_impressionsCache.HasReachedMaxSize())
            {
                Logger.Warn("Split SDK impressions queue is full. Impressions may have been dropped. Consider increasing capacity.");
            }

            var impressions = _impressionsCache.FetchAllAndClear();

            if (impressions.Count > 0)
            {
                try
                {
                    _apiClient.SendBulkImpressions(impressions);
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught updating impressions.", e);
                }
            }
        }
    }
}