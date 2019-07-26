using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Client.Classes
{
    public class SplitManager : ISplitManager
    {
        private readonly ILog _log;
        private readonly ISplitCache _splitCache;
        private readonly ISplitNameValidator _splitNameValidator;
        private readonly IBlockUntilReadyService _blockUntilReadyService;

        public SplitManager(ISplitCache splitCache,
            IBlockUntilReadyService blockUntilReadyService,
            ILog log = null)
        {
            _splitCache = splitCache;
            _log = log ?? LogManager.GetLogger(typeof(SplitManager));
            _splitNameValidator = new SplitNameValidator(_log);
            _blockUntilReadyService = blockUntilReadyService;
        }

        public List<SplitView> Splits()
        {
            if (!IsSdkReady(nameof(Splits)) || _splitCache == null)
            {
                return null;
            }

            var currentSplits = _splitCache.GetAllSplits().Cast<ParsedSplit>();

            var lightSplits = currentSplits.Select(x =>
                new SplitView()
                {
                    name = x.name,
                    killed = x.killed,
                    changeNumber = x.changeNumber,
                    treatments = (x.conditions.Where(z => z.conditionType == ConditionType.ROLLOUT).FirstOrDefault() ?? x.conditions.FirstOrDefault())?.partitions.Select(y => y.treatment).ToList(),
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

            var split = (ParsedSplit)_splitCache.GetSplit(featureName);

            if (split == null)
            {
                _log.Warn($"split: you passed {featureName} that does not exist in this environment, please double check what Splits exist in the web console.");

                return null;
            }

            var condition = split.conditions.Where(x => x.conditionType == ConditionType.ROLLOUT).FirstOrDefault() ?? split.conditions.FirstOrDefault();

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

            var currentSplits = _splitCache.GetAllSplits().Cast<ParsedSplit>();

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
