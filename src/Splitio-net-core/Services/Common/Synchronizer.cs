using Splitio.Services.Events.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using Splitio.Services.SplitFetcher.Interfaces;

namespace Splitio.Services.Common
{
    public class Synchronizer : ISynchronizer
    {
        private readonly ISplitFetcher _splitFetcher;
        private readonly ISelfRefreshingSegmentFetcher _segmentFetcher;
        private readonly IImpressionsLog _impressionsLog;
        private readonly IEventsLog _eventsLog;
        private readonly IMetricsLog _metricsLog;
        private readonly IWrapperAdapter _wrapperAdapter;
        private readonly ISplitLogger _log;

        public Synchronizer(ISplitFetcher splitFetcher,
            ISelfRefreshingSegmentFetcher segmentFetcher,
            IImpressionsLog impressionsLog,
            IEventsLog eventsLog,
            IMetricsLog metricsLog,
            IWrapperAdapter wrapperAdapter = null,
            ISplitLogger log = null)
        {
            _splitFetcher = splitFetcher;
            _segmentFetcher = segmentFetcher;
            _impressionsLog = impressionsLog;
            _eventsLog = eventsLog;
            _metricsLog = metricsLog;
            _wrapperAdapter = wrapperAdapter ?? new WrapperAdapter();
            _log = log ?? WrapperAdapter.GetLogger(typeof(Synchronizer));
        }

        #region Public Methods
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
            _segmentFetcher.Start();
            _log.Debug("Spltis and Segments fetchers started...");
        }

        public void StopPeriodicDataRecording()
        {
            _impressionsLog.Stop();
            _eventsLog.Stop();
            _metricsLog.Clear();
            _log.Debug("Periodic Data Recording stoped...");
        }

        public void StopPeriodicFetching(bool isDestroy = false)
        {
            _splitFetcher.Stop();
            _segmentFetcher.Stop();
            _log.Debug("Spltis and Segments fetchers stoped...");
        }

        public void ClearFetchersCache()
        {
            _splitFetcher.Clear();
            _segmentFetcher.Clear();
        }

        public void SyncAll()
        {
            _splitFetcher.FetchSplits();
            _segmentFetcher.FetchAll();
            _log.Debug("Spltis and Segments synchronized...");
        }

        public void SynchronizeSegment(string segmentName)
        {
            _segmentFetcher.Fetch(segmentName);
            _log.Debug($"Segment fetched: {segmentName}...");
        }

        public void SynchronizeSplits()
        {
            _splitFetcher.FetchSplits();
            _log.Debug("Splits fetched...");
        }
        #endregion
    }
}
