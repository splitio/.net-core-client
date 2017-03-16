using Splitio.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.EngineEvaluator
{
    public class Splitter
    {
        private const string Control = "control";

        public string GetTreatment(string key, int seed, List<PartitionDefinition> partitions)
        {
            if(string.IsNullOrEmpty(key))
            {
                return Control; 
            }

            if (partitions.Count() == 1 && partitions.First().size == 100)
            {
                return partitions.First().treatment;
            }

            var bucket = Bucket(key, seed);

            var covered = 0;
            foreach(PartitionDefinition partition in partitions)
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
            return Math.Abs(Hash(key, seed) % 100) + 1;
        }

        public int Hash(string key, int seed)
        {
            int h = 0;
            for (int i = 0; i< key.Length; i++)
            {
                h = 31 * h + key[i];
            }
            return h ^ seed;
        }
    }
}
