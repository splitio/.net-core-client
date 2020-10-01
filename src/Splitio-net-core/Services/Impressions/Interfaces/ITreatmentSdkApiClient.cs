using Splitio.Domain;
using Splitio.Services.Impressions.Classes;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface ITreatmentSdkApiClient
    {
        void SendBulkImpressions(List<KeyImpression> impressions);
        void SendImpressionsCount(ConcurrentDictionary<KeyCache, int> impressionsCount);
    }
}
