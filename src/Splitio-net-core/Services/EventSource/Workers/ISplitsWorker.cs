namespace Splitio.Services.EventSource.Workers
{
    public interface ISplitsWorker : IWorker
    {
        void AddToQueue(long changeNumber);
        void KillSplit(long changeNumber, string splitName, string defaultTreatment);   
    }
}
