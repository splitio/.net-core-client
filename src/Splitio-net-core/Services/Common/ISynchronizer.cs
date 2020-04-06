namespace Splitio.Services.Common
{
    public interface ISynchronizer
    {
        void SyncAll();
        void SynchorizeSplits();
        void SynchorizeSegment(string segmentName);
        void StartPeriodicFetching();
        void StopPeriodicFetching();
        void StartPeriodicDataRecording();
        void StopPeriodicDataRecording();
    }
}
