using Splitio.Domain;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface IImpressionListener : IListener<IList<KeyImpression>>
    {
    }
}
