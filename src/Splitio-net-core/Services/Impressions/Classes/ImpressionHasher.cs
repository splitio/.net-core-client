using Murmur;
using Splitio.Domain;
using Splitio.Services.Impressions.Interfaces;
using System;
using System.Text;

namespace Splitio.Services.Impressions.Classes
{
    public class ImpressionHasher : IImpressionHasher
    {
        public ulong Process(KeyImpression impression)
        {
            var key = $"{UnknowIfNull(impression.keyName)}:{UnknowIfNull(impression.feature)}:{UnknowIfNull(impression.treatment)}:{UnknowIfNull(impression.label)}:{ZeroIfNull(impression.changeNumber)}";

            return Hash(key, 0);
        }

        public ulong Hash(string key, uint seed)
        {
            Murmur128 murmur128 = MurmurHash.Create128(seed: seed, preference: AlgorithmPreference.X64);
            byte[] keyToBytes = Encoding.ASCII.GetBytes(key);
            byte[] seedResult = murmur128.ComputeHash(keyToBytes);

            return BitConverter.ToUInt64(seedResult, 0);
        }

        private string UnknowIfNull(string value)
        {
            return string.IsNullOrEmpty(value) ? "UNKNOWN" : value;
        }

        private long ZeroIfNull(long? value)
        {
            return value == null ? 0 : value.Value;
        }
    }
}
