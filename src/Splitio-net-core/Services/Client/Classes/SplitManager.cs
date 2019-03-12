using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.InputValidation.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Client.Classes
{
    public class SplitManager : ISplitManager
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(SplitManager));
        private readonly ISplitCache splitCache;
        private readonly ISplitNameValidator _splitNameValidator;

        public SplitManager(ISplitCache splitCache)
        {
            this.splitCache = splitCache;
            _splitNameValidator = new SplitNameValidator(Log);
        }

        public List<SplitView> Splits()
        {
            if (splitCache == null)
            {
                return null;
            }

            var currentSplits = splitCache.GetAllSplits().Cast<ParsedSplit>();

            var lightSplits = currentSplits.Select(x =>
                new SplitView()
                {
                    name = x.name,
                    killed = x.killed,
                    changeNumber = x.changeNumber,
                    treatments = (x.conditions.Where(z => z.conditionType == ConditionType.ROLLOUT).FirstOrDefault() ?? new ConditionWithLogic() { partitions = new List<PartitionDefinition>() }).partitions.Select(y => y.treatment).ToList(),
                    trafficType = x.trafficTypeName
                });

            return lightSplits.ToList();
        }


        public SplitView Split(string featureName)
        {
            var result = _splitNameValidator.SplitNameIsValid(featureName, nameof(Split));

            if (splitCache == null || !result.Success)
            {
                return null;
            }

            featureName = result.Value;

            var split = (ParsedSplit)splitCache.GetSplit(featureName);

            if (split == null)
            {
                return null;
            }

            var condition = split.conditions.Where(x => x.conditionType == ConditionType.ROLLOUT).FirstOrDefault();

            var treatments = condition != null ? condition.partitions.Select(y => y.treatment).ToList() : new List<string>();

            var lightSplit = new SplitView()
            {
                name = split.name,
                killed = split.killed,
                changeNumber = split.changeNumber,
                treatments = treatments,
                trafficType = split.trafficTypeName
            };

            return lightSplit;
        }


        public List<string> SplitNames()
        {
            if (splitCache == null)
            {
                return null;
            }

            var currentSplits = splitCache.GetAllSplits().Cast<ParsedSplit>();

            return currentSplits.Select(x => x.name).ToList();
        }
    }
}
