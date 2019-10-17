﻿using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.SegmentFetcher.Interfaces;
using Splitio.Services.Shared.Classes;
using System;
using System.Linq;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public class SelfRefreshingSegment
    {
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(SelfRefreshingSegment));

        public readonly string name;
        private readonly IReadinessGatesCache gates;
        private readonly ISegmentChangeFetcher segmentChangeFetcher;
        private readonly ISegmentCache segmentCache;

        public SelfRefreshingSegment(string name, ISegmentChangeFetcher segmentChangeFetcher, IReadinessGatesCache gates, ISegmentCache segmentCache)
        {
            this.name = name;
            this.segmentChangeFetcher = segmentChangeFetcher;
            this.segmentCache = segmentCache;
            this.gates = gates;
            gates.RegisterSegment(name);
        }

        public async void RefreshSegment()
        {
            while (true)
            {
                var changeNumber = segmentCache.GetChangeNumber(name);

                try
                {
                    var response = await segmentChangeFetcher.Fetch(name, changeNumber);
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
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug(string.Format("Segment {0} - Added : {1}", name, string.Join(" - ", response.added)));
                            }
                        }
                        if (response.removed.Count() > 0)
                        {
                            if (Log.IsDebugEnabled)
                            {
                                Log.Debug(string.Format("Segment {0} - Removed : {1}", name, string.Join(" - ", response.removed)));
                            }
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
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(string.Format("segment {0} fetch before: {1}, after: {2}", name, changeNumber, segmentCache.GetChangeNumber(name)));
                    }
                }
            }
        }
    }
}
