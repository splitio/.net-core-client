namespace Splitio.Services.Client.Interfaces
{
    public interface ISplitFactory
    {
        ISplitClient Client();
        ISplitManager Manager();
    }
}
