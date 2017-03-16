namespace Splitio.Services.SplitFetcher.Interfaces
{
    public interface ISegmentSdkApiClient
    {
        string FetchSegmentChanges(string name, long since);
    }
}
