namespace Splitio.Services.Common
{
    public interface ISynchronizer
    {
        void SyncAll();
        void SynchronizeSplits();
        void SynchronizeSegment(string segmentName);
        void StartPeriodicFetching();
        void StopPeriodicFetching(bool isDestroy = false);
        void StartPeriodicDataRecording();
        void StopPeriodicDataRecording();
    }
}
