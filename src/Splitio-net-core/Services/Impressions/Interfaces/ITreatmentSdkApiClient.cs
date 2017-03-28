namespace Splitio.Services.Impressions.Interfaces
{
    public interface ITreatmentSdkApiClient
    {
        void SendBulkImpressions(string impressions);
    }
}
