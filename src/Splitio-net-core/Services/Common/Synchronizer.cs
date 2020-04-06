using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using Splitio.Services.SplitFetcher.Interfaces;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public class Synchronizer : ISynchronizer
    {
        private readonly ISplitFetcher _splitFetcher;
        private readonly ISelfRefreshingSegmentFetcher _segmentFetcher;
        private readonly IImpressionsLog _impressionsLog;
        private readonly IEventsLog _eventsLog;
        private readonly IMetricsLog _metricsLog;
        private readonly IReadinessGatesCache _gates;
        private readonly IWrapperAdapter _wrapperAdapter;
        private readonly ISplitLogger _log;

        public Synchronizer(ISplitFetcher splitFetcher,
            ISelfRefreshingSegmentFetcher segmentFetcher,
            IImpressionsLog impressionsLog,
            IEventsLog eventsLog,
            IMetricsLog metricsLog,
            IReadinessGatesCache gates,
            IWrapperAdapter wrapperAdapter = null,
            ISplitLogger log = null)
        {
            _splitFetcher = splitFetcher;
            _segmentFetcher = segmentFetcher;
            _impressionsLog = impressionsLog;
            _eventsLog = eventsLog;
            _metricsLog = metricsLog;
            _gates = gates;
            _wrapperAdapter = wrapperAdapter ?? new WrapperAdapter();
            _log = log ?? WrapperAdapter.GetLogger(typeof(Synchronizer));
        }

        public void StartPeriodicDataRecording()
        {
            _impressionsLog.Start();
            _eventsLog.Start();
            _metricsLog.Start();
            _log.Debug("Periodic Data Recording started...");
        }

        public void StartPeriodicFetching()
        {
            _splitFetcher.Start();
            StartFetchSegmentsTask();
            _log.Debug("Spltis and Segments fetchers started...");
        }

        public void StopPeriodicDataRecording()
        {
            _impressionsLog.Stop();
            _eventsLog.Stop();
            _metricsLog.Clear();
            _log.Debug("Periodic Data Recording stoped...");
        }

        public void StopPeriodicFetching()
        {
            _splitFetcher.Stop();
            _segmentFetcher.Stop();
            _log.Debug("Spltis and Segments fetchers stoped...");
        }

        public void SyncAll()
        {
            _splitFetcher.FetchSplits();
            _segmentFetcher.FetchSegments();
            _log.Debug("Spltis and Segments synchronized...");
        }

        public void SynchorizeSegment(string segmentName)
        {
            _segmentFetcher.FetchSegment(segmentName);
            _log.Debug($"Segment fetched: {segmentName}...");
        }

        public void SynchorizeSplits()
        {
            _splitFetcher.FetchSplits();
            _log.Debug("Splits fetched...");
        }

        #region Private Methods
        private void StartFetchSegmentsTask()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_gates.IsSDKReady(0))
                    {
                        _segmentFetcher.Start();
                        break;
                    }

                    _wrapperAdapter.TaskDelay(500).Wait();
                }
            });
        }
        #endregion
    }
}
