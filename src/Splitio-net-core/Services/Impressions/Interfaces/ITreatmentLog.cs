namespace Splitio.Services.Impressions.Interfaces
{
    public interface ITreatmentLog
    {
        void Log(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey = null);
    }
}
