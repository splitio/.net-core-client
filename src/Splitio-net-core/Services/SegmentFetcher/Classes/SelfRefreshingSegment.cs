using log4net;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.SegmentFetcher.Interfaces;
using System;
using System.Linq;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public class SelfRefreshingSegment
    {
        private static readonly ILog Log = LogManager.GetLogger("splitio",typeof(SelfRefreshingSegment));

        public string name;
        private IReadinessGatesCache gates;
        private ISegmentChangeFetcher segmentChangeFetcher;
        private ISegmentCache segmentCache;

        public SelfRefreshingSegment(string name, ISegmentChangeFetcher segmentChangeFetcher, IReadinessGatesCache gates,  ISegmentCache segmentCache)
        {
            this.name = name;
            this.segmentChangeFetcher = segmentChangeFetcher;
            this.segmentCache = segmentCache;
            this.gates = gates;
            gates.RegisterSegment(name);
        }

        public void RefreshSegment()
        {        
            while (true)
            {
                var changeNumber = segmentCache.GetChangeNumber(name);

                try
                {
                    var response = segmentChangeFetcher.Fetch(name, changeNumber);
                    if (response == null)
                    {
                        break;
                    }
                    if (changeNumber >= response.till)
                    {
                        gates.SegmentIsReady(name);
                        break;
                    }

                    if (response.added.Count() > 0 || response.removed.Count() > 0)
                    {

                        segmentCache.AddToSegment(name, response.added);
                        segmentCache.RemoveFromSegment(name, response.removed);

                        if (response.added.Count() > 0)
                        {
                            Log.Info(string.Format("Segment {0} - Added : {1}", name, string.Join(" - ", response.added)));
                        }
                        if (response.removed.Count() > 0)
                        {
                            Log.Info(string.Format("Segment {0} - Removed : {1}", name, string.Join(" - ", response.removed)));
                        }
                    }

                    segmentCache.SetChangeNumber(name, response.till);                  
                }
                catch (Exception e)
                {
                    Log.Error("Exception caught refreshing segment", e);
                }
                finally
                {
                    Log.Info(string.Format("segment {0} fetch before: {1}, after: {2}", name, changeNumber, segmentCache.GetChangeNumber(name)));
                }
            }
        }
    }
}
