namespace Splitio.Services.EventSource.Workers
{
    public interface ISegmentsWorker : IWorker
    {
        void AddToQueue(long changeNumber, string segmentName);
    }
}
