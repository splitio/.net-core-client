﻿using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Threading;

namespace Splitio.Services.Impressions.Classes
{
    public class SelfUpdatingTreatmentLog : IListener<KeyImpression>
    {
        private ITreatmentSdkApiClient apiClient;
        private int interval;
        private ISimpleProducerCache<KeyImpression> impressionsCache;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected static readonly ILog Logger = LogManager.GetLogger(typeof(SelfUpdatingTreatmentLog));

        public SelfUpdatingTreatmentLog(ITreatmentSdkApiClient apiClient, int interval, ISimpleCache<KeyImpression> impressionsCache, int maximumNumberOfKeysToCache = -1)
        {
            this.impressionsCache = (impressionsCache as ISimpleProducerCache<KeyImpression> ) ?? new InMemorySimpleCache<KeyImpression>(new BlockingQueue<KeyImpression>(maximumNumberOfKeysToCache));
            this.apiClient = apiClient;
            this.interval = interval;
        }

        public void Start()
        {
            PeriodicTaskFactory.Start(() => { SendBulkImpressions(); }, interval * 1000, cancellationTokenSource.Token);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            SendBulkImpressions();
        }


        private void SendBulkImpressions()
        {
            if (impressionsCache.HasReachedMaxSize())
            {
                Logger.Warn("Split SDK impressions queue is full. Impressions may have been dropped. Consider increasing capacity.");
            }

            var impressions = impressionsCache.FetchAllAndClear();

            if (impressions.Count > 0)
            {
                try
                {
                    apiClient.SendBulkImpressions(impressions);
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught updating impressions.", e);
                }
            }
        }

        public void Log(KeyImpression impression)
        {
            impressionsCache.AddItem(impression);
        }
    }
}