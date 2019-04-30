using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Redis.Services.Client.Classes
{
    public class RedisSplitManager : ISplitManager
    {
        ISplitCache splitCache;

        public RedisSplitManager(ISplitCache splitCache)
        {
            this.splitCache = splitCache;
        }

        public List<SplitView> Splits()
        {
            if (splitCache == null)
            {
                return null;
            }

            var currentSplits = splitCache.GetAllSplits().Cast<Split>();

            var lightSplits = currentSplits.Select(x =>
                new SplitView()
                {
                    name = x.name,
                    killed = x.killed,
                    changeNumber = x.changeNumber,
                    treatments = (x.conditions.Where(z => z.conditionType.ToUpper() == "ROLLOUT").FirstOrDefault() ?? new ConditionDefinition() { partitions = new List<PartitionDefinition>() }).partitions.Select(y => y.treatment).ToList(),
                    trafficType = x.trafficTypeName,
                    configs = x.configurations
                });

            return lightSplits.ToList();
        }


        public SplitView Split(string featureName)
        {
            if (splitCache == null)
            {
                return null;
            }

            var split = (Split)splitCache.GetSplit(featureName);

            if (split == null)
            {
                return null;
            }

            var condition = split.conditions.Where(x => x.conditionType.ToUpper() == "ROLLOUT").FirstOrDefault();
            
            var treatments = condition != null ? condition.partitions.Select(y => y.treatment).ToList() : new List<string>();

            var lightSplit = new SplitView()
            {
                name = split.name,
                killed = split.killed,
                changeNumber = split.changeNumber,
                treatments = treatments,
                trafficType = split.trafficTypeName,
                configs = x.configurations
            };

            return lightSplit;
        }


        public List<string> SplitNames()
        {
            if (splitCache == null)
            {
                return null;
            }

            var currentSplits = splitCache.GetAllSplits().Cast<Split>();

            return currentSplits.Select(x => x.name).ToList();
        }
    }
}
