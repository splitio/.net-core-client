using Splitio.Domain;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface IImpressionListener
    {
        void Log(KeyImpression impression);
    }
}
