using log4net;
using Newtonsoft.Json;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Splitio.Services.Impressions.Classes
{
    public class SelfUpdatingTreatmentLog: ITreatmentLog
    {
        private ITreatmentSdkApiClient apiClient;
        private int interval;
        private IImpressionsCache impressionsCache;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected static readonly ILog Logger = LogManager.GetLogger("splitio",typeof(SelfUpdatingTreatmentLog));

        public SelfUpdatingTreatmentLog(ITreatmentSdkApiClient apiClient, int interval, IImpressionsCache impressionsCache, int maximumNumberOfKeysToCache = -1)
        {
            this.impressionsCache = impressionsCache ?? new InMemoryImpressionsCache(new BlockingQueue<KeyImpression>(maximumNumberOfKeysToCache));
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
        }


        private void SendBulkImpressions()
        {
            if(((InMemoryImpressionsCache)impressionsCache).HasReachedMaxSize())
            {
                Logger.Warn("Split SDK impressions queue is full. Impressions may have been dropped. Consider increasing capacity.");
            }

            var impressions = impressionsCache.FetchAllAndClear();

            if (impressions.Count > 0)
            {
                try
                {
                    var impressionsJson = ConvertToJson(impressions);
                    apiClient.SendBulkImpressions(impressionsJson);
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught updating impressions.", e);
                }
            }
        }

        private string ConvertToJson(List<KeyImpression> impressions)
        {
            var impressionsPerFeature = 
                impressions
                .GroupBy(item => item.feature)
                .Select(group => new { testName = group.Key, keyImpressions = group.Select(x => new { keyName = x.keyName, treatment = x.treatment, time = x.time, changeNumber = x.changeNumber, label = x.label, bucketingKey = x.bucketingKey }) });
            return JsonConvert.SerializeObject(impressionsPerFeature);
        }


        public void Log(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey = null)
        {
            KeyImpression impression = new KeyImpression() { feature = feature, keyName = matchingKey, treatment = treatment, time = time, changeNumber = changeNumber, label = label, bucketingKey = bucketingKey };
            var enqueueTask = new Task(() => impressionsCache.AddImpression(impression));
            enqueueTask.Start();
        }
    }
}
