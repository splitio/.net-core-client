using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using System;
using System.Linq;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public class SelfRefreshingSegment : ISelfRefreshingSegment
    {
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(SelfRefreshingSegment));

        public string Name;
        private readonly IReadinessGatesCache _gates;
        private readonly ISegmentChangeFetcher _segmentChangeFetcher;
        private readonly ISegmentCache _segmentCache;

        public SelfRefreshingSegment(string name, ISegmentChangeFetcher segmentChangeFetcher, IReadinessGatesCache gates, ISegmentCache segmentCache)
        {
            Name = name;

            _segmentChangeFetcher = segmentChangeFetcher;
            _segmentCache = segmentCache;
            _gates = gates;
            _gates.RegisterSegment(name);
        }

        public void FetchSegment(string segmentName)
        {
            Name = segmentName;
            _gates.RegisterSegment(Name);
            FetchSegment();
        }

        public async void FetchSegment()
        {
            while (true)
            {
                var changeNumber = _segmentCache.GetChangeNumber(Name);

                try
                {
                    var response = await _segmentChangeFetcher.Fetch(Name, changeNumber);
                    if (response == null)
                    {
                        break;
                    }
                    if (changeNumber >= response.till)
                    {
                        _gates.SegmentIsReady(Name);
                        break;
                    }

                    if (response.added.Count() > 0 || response.removed.Count() > 0)
                    {
                        _segmentCache.AddToSegment(Name, response.added);
                        _segmentCache.RemoveFromSegment(Name, response.removed);

                        if (response.added.Count() > 0)
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug(string.Format("Segment {0} - Added : {1}", Name, string.Join(" - ", response.added)));
                            }
                        }
                        if (response.removed.Count() > 0)
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug(string.Format("Segment {0} - Removed : {1}", Name, string.Join(" - ", response.removed)));
                            }
                        }
                    }

                    _segmentCache.SetChangeNumber(Name, response.till);
                }
                catch (Exception e)
                {
                    Log.Error("Exception caught refreshing segment", e);
                }
                finally
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(string.Format("segment {0} fetch before: {1}, after: {2}", Name, changeNumber, _segmentCache.GetChangeNumber(Name)));
                    }
                }
            }
        }
    }
}
