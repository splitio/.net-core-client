using Murmur;
using Splitio.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Splitio.Services.EngineEvaluator
{
    public class Splitter
    {
        private const string Control = "control";

        public virtual string GetTreatment(string key, int seed, List<PartitionDefinition> partitions, AlgorithmEnum algorithm)
        {
            if (String.IsNullOrEmpty(key))
            {
                return Control;
            }

            if (partitions.Count() == 1 && partitions.First().size == 100)
            {
                return partitions.First().treatment;
            }

            var bucket = algorithm == AlgorithmEnum.LegacyHash ? LegacyBucket(key, seed) : Bucket(key, seed);

            var covered = 0;
            foreach (PartitionDefinition partition in partitions)
            {
                covered += partition.size;
                if (covered >= bucket)
                {
                    return partition.treatment;
                }
            }

            return Control;
        }

        public int Bucket(string key, int seed)
        {
            return (int)Math.Abs(Hash(key, seed) % 100) + 1;
        }

        public int LegacyBucket(string key, int seed)
        {
            return Math.Abs(LegacyHash(key, seed) % 100) + 1;
        }

        public long Hash(string key, int seed)
        {
            var unsignedSeed = (uint)seed;
            HashAlgorithm hashAlgorithm = MurmurHash.Create32(unsignedSeed); // returns a managed 32-bit algorithm with seed
            byte[] keyToBytes = Encoding.UTF8.GetBytes(key);
            byte[] seedResult = hashAlgorithm.ComputeHash(keyToBytes, 0, keyToBytes.Length);
            var result = BitConverter.ToUInt32(seedResult, 0);

            return result;
        }

        public int LegacyHash(string key, int seed)
        {
            int h = 0;
            for (int i = 0; i < key.Length; i++)
            {
                h = 31 * h + key[i];
            }
            return h ^ seed;
        }
    }
}
