using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Splitio.Services.Client.Classes
{
    public class InMemoryReadinessGatesCache : IReadinessGatesCache
    {
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(InMemoryReadinessGatesCache));

        private readonly CountdownEvent splitsAreReady = new CountdownEvent(1);
        private readonly Dictionary<string, CountdownEvent> segmentsAreReady = new Dictionary<string, CountdownEvent>();
        private readonly Stopwatch _splitsReadyTimer = new Stopwatch();
        private readonly Stopwatch _segmentsReadyTimer = new Stopwatch();

        public InMemoryReadinessGatesCache()
        {
            _segmentsReadyTimer.Start();
            _splitsReadyTimer.Start();
        }

        public bool IsSDKReady(int milliseconds)
        {
            Stopwatch clock = new Stopwatch();
            clock.Start();

            if (!AreSplitsReady(milliseconds))
            {
                return false;
            }

            int timeLeft = milliseconds - (int)clock.ElapsedMilliseconds;

            return AreSegmentsReady(timeLeft);
        }


        public void SplitsAreReady()
        {
            if (!splitsAreReady.IsSet)
            {
                splitsAreReady.Signal();
                if (splitsAreReady.IsSet)
                {
                    _splitsReadyTimer.Stop();
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug($"Splits are ready in {_splitsReadyTimer.ElapsedMilliseconds} milliseconds");
                    }
                }
            }
        }

        public void SegmentIsReady(string segmentName)
        {
            CountdownEvent countDown;
            segmentsAreReady.TryGetValue(segmentName, out countDown);

            if ((countDown == null) || (countDown.IsSet))
            {
                return;
            }

            countDown.Signal();

            if (countDown.IsSet)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(segmentName + " segment is ready");
                }
            }
        }

        public bool AreSplitsReady(int milliseconds)
        {
            return splitsAreReady.Wait(milliseconds);
        }

        public bool RegisterSegment(string segmentName)
        {
            if (string.IsNullOrEmpty(segmentName) || AreSplitsReady(0))
            {
                return false;
            }

            try
            {
                segmentsAreReady.Add(segmentName, new CountdownEvent(1));
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Registered segment: " + segmentName);
                }
            }
            catch (ArgumentException e)
            {
                Log.Warn("Already registered segment: " + segmentName, e);
            }

            return true;
        }

        public bool AreSegmentsReady(int milliseconds)
        {
            Stopwatch clock = new Stopwatch();
            clock.Start();
            int timeLeft = milliseconds;

            foreach (var entry in segmentsAreReady)
            {
                var segmentName = entry.Key;
                var countdown = entry.Value;

                if (timeLeft >= 0)
                {
                    if (!countdown.Wait(timeLeft))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!countdown.Wait(0))
                    {
                        return false;
                    }
                }
                timeLeft = timeLeft - (int)clock.ElapsedMilliseconds;
            }

            _segmentsReadyTimer.Stop();
            if (Log.IsDebugEnabled)
            {
                Log.Debug($"Segments are ready in {_segmentsReadyTimer.ElapsedMilliseconds} milliseconds");
            }

            return true;
        }
    }
}
