namespace Splitio.Services.SplitFetcher.Interfaces
{
    public interface ISplitSdkApiClient
    {
        string FetchSplitChanges(long since);
    }
}
