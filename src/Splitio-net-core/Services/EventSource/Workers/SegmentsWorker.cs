using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;

namespace Splitio.Services.EventSource.Workers
{
    public class SegmentsWorker : Worker<SegmentQueueDto>
    {
        private readonly ISegmentCache _segmentCache;
        private readonly ISynchronizer _synchronizer;

        public SegmentsWorker(ISegmentCache segmentCache,
            ISynchronizer synchronizer,
            ISplitLogger log = null) : base (log ?? WrapperAdapter.GetLogger(typeof(SegmentsWorker)), "Segments")
        {
            _segmentCache = segmentCache;
            _synchronizer = synchronizer;
        }     

        protected override async void ForceRefreshAsync(SegmentQueueDto segment)
        {
            if (_segmentCache.GetChangeNumber(segment.SegmentName) >= segment.ChangeNumber) return;

            await _synchronizer.SynchronizeSegment(segment.SegmentName);            
        }
    }
}
