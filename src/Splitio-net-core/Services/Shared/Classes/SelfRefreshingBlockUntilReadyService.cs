using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Events.Classes;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.Shared.Interfaces;
using Splitio.Services.SplitFetcher.Classes;
using System;
using System.Threading.Tasks;

namespace Splitio.Services.Shared.Classes
{
    public class SelfRefreshingBlockUntilReadyService : IBlockUntilReadyService
    {
        public bool Ready { get; set; }

        private readonly SelfRefreshingSplitFetcher _splitFetcher;
        private readonly SelfRefreshingSegmentFetcher _selfRefreshingSegmentFetcher;
        private readonly IReadinessGatesCache _gates;        
        private readonly IListener<KeyImpression> _treatmentLog;
        private readonly IListener<WrappedEvent> _eventLog;
        private readonly ILog _log;

        public SelfRefreshingBlockUntilReadyService(IReadinessGatesCache gates,
            SelfRefreshingSplitFetcher splitFetcher,
            SelfRefreshingSegmentFetcher selfRefreshingSegmentFetcher,
            IListener<KeyImpression> treatmentLog,
            IListener<WrappedEvent> eventLog, 
            ILog log)
        {
            _gates = gates;
            _splitFetcher = splitFetcher;
            _selfRefreshingSegmentFetcher = selfRefreshingSegmentFetcher;
            _treatmentLog = treatmentLog;
            _eventLog = eventLog;
            _log = log;
        }

        public void BlockUntilReady(int blockMilisecondsUntilReady)
        {
            if (!Ready)
            {
                if (blockMilisecondsUntilReady <= 0)
                {
                    _log.Warn("The blockMilisecondsUntilReady param has to be higher than 0.");
                }

                Start();

                Ready = _gates.IsSDKReady(blockMilisecondsUntilReady);

                if (!Ready)
                {
                    throw new TimeoutException(string.Format($"SDK was not ready in {blockMilisecondsUntilReady} miliseconds"));
                }

                LaunchTaskSchedulerOnReady();
            }
        }

        public bool IsSdkReady()
        {
            return Ready;
        }

        private void Start()
        {
            ((SelfUpdatingTreatmentLog)_treatmentLog).Start();
            ((SelfUpdatingEventLog)_eventLog).Start();
            _splitFetcher.Start();
        }

        private void LaunchTaskSchedulerOnReady()
        {
            var workerTask = Task.Factory.StartNew(
                () => {
                    while (true)
                    {
                        if (_gates.IsSDKReady(0))
                        {
                            _selfRefreshingSegmentFetcher.StartScheduler();
                            break;
                        }

                        Task.Delay(500).Wait();
                    }
                });
        }
    }
}
