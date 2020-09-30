using System.Collections.Concurrent;

namespace Splitio.Services.Impressions.Interfaces
{
    public interface IImpressionsCounter
    {
        void Inc(string splitName, long timeFrame);
        ConcurrentDictionary<string, int> PopAll();
    }
}
