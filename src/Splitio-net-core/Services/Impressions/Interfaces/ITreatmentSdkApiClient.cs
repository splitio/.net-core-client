using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface ITreatmentSdkApiClient
    {
        void SendBulkImpressions(List<KeyImpression> impressions);
    }
}
