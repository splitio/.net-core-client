using Splitio.CommonLibraries;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;
using System.Threading;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionsCountSender : IImpressionsCountSender
    {
        // Send bulk impressions count - Refresh rate: 30 min.
        private const int CounterRefreshRateSeconds = 1800;

        protected static readonly ISplitLogger Logger = WrapperAdapter.GetLogger(typeof(ImpressionsCountSender));

        private readonly ITreatmentSdkApiClient _apiClient;
        private readonly IImpressionsCounter _impressionsCounter;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _interval;

        private bool _running;

        public ImpressionsCountSender(ITreatmentSdkApiClient apiClient,
            IImpressionsCounter impressionsCounter,
            int? interval = null)
        {
            _apiClient = apiClient;
            _impressionsCounter = impressionsCounter;            
            _cancellationTokenSource = new CancellationTokenSource();
            _interval = interval ?? CounterRefreshRateSeconds;
            _running = false;
        }

        public void Start()
        {
            PeriodicTaskFactory.Start(() => { SendBulkImpressionsCount(); _running = true; }, CounterRefreshRateSeconds * 1000, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (!_running)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            SendBulkImpressionsCount();
        }

        private void SendBulkImpressionsCount()
        {
            var impressions = _impressionsCounter.PopAll();

            if (impressions.Count > 0)
            {
                try
                {
                    _apiClient.SendBulkImpressionsCount(impressions);
                }
                catch (Exception e)
                {
                    Logger.Error("Exception caught sending impressions count.", e);
                }
            }
        }
    }
}
