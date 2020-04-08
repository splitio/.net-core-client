namespace Splitio.Services.Common
{
    public interface ISynchronizer
    {
        void SyncAll();
        void SynchronizeSplits();
        void SynchronizeSegment(string segmentName);
        void StartPeriodicFetching();
        void StopPeriodicFetching();
        void StartPeriodicDataRecording();
        void StopPeriodicDataRecording();
    }
}
