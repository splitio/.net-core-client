namespace Splitio.Services.EventSource.Workers
{
    public interface ISplitsWorker : IWorker<long>
    {
        void KillSplit(long changeNumber, string splitName, string defaultTreatment);   
    }
}
