namespace Splitio.Services.SplitFetcher.Interfaces
{
    public interface ISplitFetcher
    {
        void Start();
        void Stop(bool isDestroy = false);
        void FetchSplits();
    }
}
