using Splitio.Domain;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface IImpressionHasher
    {
        ulong Process(KeyImpression impression);
    }
}
