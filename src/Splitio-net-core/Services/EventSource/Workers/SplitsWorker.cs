using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Common;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;

namespace Splitio.Services.EventSource.Workers
{
    public class SplitsWorker : Worker<long>, ISplitsWorker
    {
        private readonly ISplitCache _splitCache;
        private readonly ISynchronizer _synchronizer;

        public SplitsWorker(ISplitCache splitCache,
            ISynchronizer synchronizer,
            ISplitLogger log = null) : base (log ?? WrapperAdapter.GetLogger(typeof(SplitsWorker)), "Splits")
        {
            _splitCache = splitCache;
            _synchronizer = synchronizer;
        }        

        public void KillSplit(long changeNumber, string splitName, string defaultTreatment)
        {
            try
            {
                if (_queue != null)
                {
                    _log.Debug($"Kill Split: {splitName}, changeNumber: {changeNumber} and defaultTreatment: {defaultTreatment}");
                    _splitCache.Kill(changeNumber, splitName, defaultTreatment);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"KillSplit: {ex.Message}");
            }
        }

        protected override async void ForceRefreshAsync(long changeNumber)
        {
            if (_splitCache.GetChangeNumber() >= changeNumber) return;

            await _synchronizer.SynchronizeSplits();            
        }
    }
}
