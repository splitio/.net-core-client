using Splitio.Services.Metrics.Interfaces;
using System;

namespace Splitio.Services.Metrics.Classes
{
    public class BinarySearchLatencyTracker : ILatencyTracker
    {

        static long[] BUCKETS = {
            1000,    1500,    2250,   3375,    5063,
            7594,    11391,   17086,  25629,   38443,
            57665,   86498,   129746, 194620,  291929,
            437894,  656841,  985261, 1477892, 2216838,
            3325257, 4987885, 7481828
        };

        static long MAX_LATENCY = 7481828;

        long[] latencies = new long[BUCKETS.Length];


        public void AddLatencyMillis(long millis)
        {
            int index = FindIndex(millis * 1000);
            latencies[index]++;
        }


        public void AddLatencyMicros(long micros)
        {
            int index = FindIndex(micros);
            latencies[index]++;
        }

        public long[] GetLatencies()
        {
            return latencies;
        }

        public long GetLatency(int index)
        {
            return latencies[index];
        }

        public long GetBucketForLatencyMillis(long latency)
        {
            return latencies[FindIndex(latency * 1000)];
        }

        public long GetBucketForLatencyMicros(long latency)
        {
            return latencies[FindIndex(latency)];
        }

        public int FindIndex(long micros)
        {
            if (micros > MAX_LATENCY)
            {
                return BUCKETS.Length - 1;
            }

            int index = Array.BinarySearch(BUCKETS, micros);

            if (index < 0)
            {
                //When index is negative, do bitwise negation
                index = ~index; 
            }
            return index;
        }

        public void SetLatencyCount(int index, long count)
        {
            latencies[index] = count;
        }
    }
}
