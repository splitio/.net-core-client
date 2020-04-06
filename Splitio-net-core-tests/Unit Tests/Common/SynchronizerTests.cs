using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Metrics.Interfaces;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.SplitFetcher.Interfaces;

namespace Splitio_Tests.Unit_Tests.Common
{
    [TestClass]
    public class SynchronizerTests
    {
        private readonly Mock<ISplitFetcher> _splitFetcher;
        private readonly Mock<ISelfRefreshingSegmentFetcher> _segmentFetcher;
        private readonly Mock<IImpressionsLog> _impressionsLog;
        private readonly Mock<IEventsLog> _eventsLog;
        private readonly Mock<IMetricsLog> _metricsLog;
        private readonly Mock<IReadinessGatesCache> _gates;
        private readonly Mock<ISplitLogger> _log;
        private readonly ISynchronizer synchronizer;

        public SynchronizerTests()
        {
            _splitFetcher = new Mock<ISplitFetcher>();
            _segmentFetcher = new Mock<ISelfRefreshingSegmentFetcher>();
            _impressionsLog = new Mock<IImpressionsLog>();
            _eventsLog = new Mock<IEventsLog>();
            _metricsLog = new Mock<IMetricsLog>();
            _gates = new Mock<IReadinessGatesCache>();
            _log = new Mock<ISplitLogger>();

            synchronizer = new Synchronizer(_splitFetcher.Object, _segmentFetcher.Object, _impressionsLog.Object, _eventsLog.Object, _metricsLog.Object, _gates.Object, log: _log.Object);
        }
    }
}
