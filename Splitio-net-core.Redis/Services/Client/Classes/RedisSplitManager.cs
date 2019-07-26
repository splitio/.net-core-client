using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Redis.Services.Client.Classes
{
    public class RedisSplitManager : ISplitManager
    {
        private readonly ISplitCache _splitCache;
        private readonly IBlockUntilReadyService _blockUntilReadyService;
        private readonly ILog _log;
        private readonly ISplitNameValidator _splitNameValidator;

        public RedisSplitManager(ISplitCache splitCache,
            IBlockUntilReadyService blockUntilReadyService,
            ILog log = null)
        {
            _splitCache = splitCache;
            _blockUntilReadyService = blockUntilReadyService;
            _log = log ?? LogManager.GetLogger(typeof(RedisSplitManager));
            _splitNameValidator = new SplitNameValidator(_log);
        }

        public List<SplitView> Splits()
        {
            if (!IsSdkReady(nameof(Splits)) || _splitCache == null)
            {
                return null;
            }

            var currentSplits = _splitCache.GetAllSplits().Cast<Split>();

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
            if (!IsSdkReady(nameof(Split)) || _splitCache == null)
            {
                return null;
            }

            var result = _splitNameValidator.SplitNameIsValid(featureName, nameof(Split));

            if (!result.Success)
            {
                return null;
            }

            featureName = result.Value;

            var split = (Split)_splitCache.GetSplit(featureName);

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
                configs = split.configurations
            };

            return lightSplit;
        }


        public List<string> SplitNames()
        {
            if (!IsSdkReady(nameof(SplitNames)) || _splitCache == null)
            {
                return null;
            }

            var currentSplits = _splitCache.GetAllSplits().Cast<Split>();

            return currentSplits.Select(x => x.name).ToList();
        }

        private bool IsSdkReady(string methodName)
        {
            if (!_blockUntilReadyService.IsSdkReady())
            {
                _log.Error($"{methodName}: the SDK is not ready, the operation cannot be executed.");
                return false;
            }

            return true;
        }

        public void BlockUntilReady(int blockMilisecondsUntilReady)
        {
            _blockUntilReadyService.BlockUntilReady(blockMilisecondsUntilReady);
        }
    }
}
