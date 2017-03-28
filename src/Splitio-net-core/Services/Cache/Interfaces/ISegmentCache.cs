using System.Collections.Generic;

namespace Splitio.Services.Cache.Interfaces
{
    public interface ISegmentCache
    {
        void AddToSegment(string segmentName, List<string> segmentKeys);

        void RemoveFromSegment(string segmentName, List<string> segmentKeys);

        bool IsInSegment(string segmentName, string key);

        void SetChangeNumber(string segmentName, long changeNumber);

        long GetChangeNumber(string segmentName);
    }
}
