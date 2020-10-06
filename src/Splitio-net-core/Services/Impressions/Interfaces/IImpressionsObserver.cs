using Splitio.Domain;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface IImpressionsObserver
    {
        long? TestAndSet(KeyImpression impression);
    }
}
