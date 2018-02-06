using Splitio.Domain;
using Splitio.Services.Shared.Interfaces;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface IImpressionListener : IListener<KeyImpression>
    {
    }
}
