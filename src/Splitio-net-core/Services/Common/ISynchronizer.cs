using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public interface ISynchronizer
    {
        void SyncAll();
        Task SynchronizeSplits();
        Task SynchronizeSegment(string segmentName);
        void StartPeriodicFetching();
        void StopPeriodicFetching();
        void StartPeriodicDataRecording();
        void StopPeriodicDataRecording();
        void ClearFetchersCache();
    }
}
