namespace Splitio.Services.Cache.Interfaces
{
    public interface IReadinessGatesCache
    {
        bool AreSegmentsReady(int milliseconds);

        bool AreSplitsReady(int milliseconds);

        bool IsSDKReady(int milliseconds);

        bool RegisterSegment(string segmentName);

        void SegmentIsReady(string segmentName);

        void SplitsAreReady();
    }
}
