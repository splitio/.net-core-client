using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Metrics.Classes;
using Splitio.Services.Metrics.Interfaces;
using System;
using System.Collections.Generic;

namespace Splitio.Redis.Services.Cache.Classes
{
    public class RedisMetricsCache : RedisCacheBase, IMetricsCache
    {
        private const string metricsLatencyKeyPrefix = "latency.{metricName}.bucket.{bucketNumber}";
        private const string metricsCountKeyPrefix = "count.";
        private const string metricsGaugeKeyPrefix = "gauge.";

        private readonly ILatencyTracker _latencyTracker;        

        public RedisMetricsCache(IRedisAdapter redisAdapter, 
            string machineIP, 
            string sdkVersion, 
            string machineName, 
            string userPrefix = null) : base(redisAdapter, machineIP, sdkVersion, machineName, userPrefix) 
        {
            _latencyTracker = new BinarySearchLatencyTracker();       
        }

        public Counter IncrementCount(string name, long delta)
        {
            var key = $"{RedisKeyPrefix}{metricsCountKeyPrefix}{name}";
            var result = _redisAdapter.IcrBy(key, delta);
            var counter = new Counter(); //TODO: counter.count is losing its original value!!
            counter.AddDelta(result);

            return counter;
        }

        public Counter GetCount(string name)
        {
            var key = $"{RedisKeyPrefix}{metricsCountKeyPrefix}{name}";
            var result = _redisAdapter.Get(key);
            var counter = new Counter(); //TODO: counter.count is losing its original value!!
            counter.AddDelta(long.Parse(result));

            return counter;
        }

        public Dictionary<string, Counter> FetchAllCountersAndClear()
        {
            var result = new Dictionary<string, Counter>();
            var pattern = $"{RedisKeyPrefix}{metricsCountKeyPrefix}*";
            var keys = _redisAdapter.Keys(pattern);

            foreach (var count in keys)
            {
                var value = _redisAdapter.Get(count);
                var countName = ((string)count).Replace(RedisKeyPrefix + metricsCountKeyPrefix, "");
                var counterValue = long.Parse(value);

                result.Add(countName, new Counter(counterValue)); //TODO: counter.count is losing its original value!!

                _redisAdapter.Del(count);
            }

            return result;
        }

        public void SetGauge(string name, long gauge)
        {
            var key = $"{RedisKeyPrefix}{metricsGaugeKeyPrefix}{name}";

            _redisAdapter.Set(key, gauge.ToString());
        }

        public long GetGauge(string name)
        {
            var key = $"{RedisKeyPrefix}{metricsGaugeKeyPrefix}{name}";
            var value = _redisAdapter.Get(key);

            return long.Parse(value);
        }

        public Dictionary<string, long> FetchAllGaugesAndClear()
        {
            var result = new Dictionary<string, long>();
            var pattern = $"{RedisKeyPrefix}{metricsGaugeKeyPrefix}*";
            var keys = _redisAdapter.Keys(pattern);

            foreach (var gauge in keys)
            {
                var value = _redisAdapter.Get(gauge);
                var gaugeName = ((string)gauge).Replace(RedisKeyPrefix + metricsGaugeKeyPrefix, "");

                result.Add(gaugeName, long.Parse(value));

                _redisAdapter.Del(gauge);
            }

            return result;
        }

        public void SetLatency(string name, long value)
        {
            var bucketToIncrement = ((BinarySearchLatencyTracker)_latencyTracker).FindIndex(value * 1000);
            var key = RedisKeyPrefix + metricsLatencyKeyPrefix.Replace("{metricName}", name).Replace("{bucketNumber}", bucketToIncrement.ToString());

            _redisAdapter.IcrBy(key, 1);
        }

        public ILatencyTracker GetLatencyTracker(string name)
        {
            ILatencyTracker result = new BinarySearchLatencyTracker();

            var bucketsPattern = RedisKeyPrefix + metricsLatencyKeyPrefix.Replace("{metricName}", name).Replace("{bucketNumber}", "*");
            var keys = _redisAdapter.Keys(bucketsPattern);
            
            foreach (var key in keys)
            {
                var bucketPrefix = bucketsPattern.Replace("*", "");
                var bucket = ((string)key).Replace(bucketPrefix, "");
                var currentBucketPattern = bucketsPattern.Replace("*", bucket);
                var valueString = _redisAdapter.Get(currentBucketPattern);
                var value = long.Parse(valueString);

                result.SetLatencyCount(int.Parse(bucket), value);
            }

            return result;
        }

        public Dictionary<string, ILatencyTracker> FetchAllLatencyTrackersAndClear()
        {
            var result = new Dictionary<string, ILatencyTracker>();
            var pattern = RedisKeyPrefix + metricsLatencyKeyPrefix.Replace("{metricName}.bucket.{bucketNumber}", "*");
            var keys = _redisAdapter.Keys(pattern);

            foreach(var key in keys)
            {
                var keyParts = ((string)key).Split(new Char[]{'.', '/'});
                var latencyPosition = Array.IndexOf(keyParts, "latency");
                var bucketPosition = Array.IndexOf(keyParts, "bucket");
                string name = keyParts[latencyPosition + 1];
                string bucket = keyParts[bucketPosition + 1];

                result.TryGetValue(name, out ILatencyTracker tracker);

                if (tracker == null)
                {
                    tracker = new BinarySearchLatencyTracker();                
                    result.Add(name, tracker);
                }

                var valueString = _redisAdapter.Get(key);
                var value = long.Parse(valueString);
                
                tracker.SetLatencyCount(int.Parse(bucket), value);

                _redisAdapter.Del(key);
            }

            return result;
        }      
    }
}
