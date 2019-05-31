using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Parsing;
using Splitio.Services.Shared.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Splitio.Services.Shared.Classes
{
    public abstract class AbstractLocalhostFileService : ILocalhostFileService
    {
        protected const string Control = "control";

        protected ILog _log;

        public abstract ConcurrentDictionary<string, ParsedSplit> ParseSplitFile(string filePath);

        protected ParsedSplit CreateParsedSplit(string name, string treatment, List<ConditionWithLogic> codnitions = null)
        {
            var split = new ParsedSplit()
            {
                name = name,
                seed = 0,
                defaultTreatment = treatment,
                conditions = codnitions,
                algo = AlgorithmEnum.Murmur,
                trafficAllocation = 100
            };

            return split;
        }

        protected ConditionWithLogic CreateCondition(string treatment, List<string> keys = null)
        {
            if (keys != null)
            {
                return new ConditionWithLogic
                {
                    conditionType = ConditionType.WHITELIST,
                    matcher = new CombiningMatcher
                    {
                        combiner = CombinerEnum.AND,
                        delegates = new List<AttributeMatcher>
                        {
                            new AttributeMatcher
                            {
                                negate = false,
                                matcher = new WhitelistMatcher(keys)
                            }
                        }
                    },
                    partitions = new List<PartitionDefinition>
                    {
                        new PartitionDefinition
                        {
                            size = 100,
                            treatment = treatment
                        }
                    },
                    label = $"whitelisted {string.Join(", ", keys)}"
                };
            }
            else
            {
                return new ConditionWithLogic
                {
                    conditionType = ConditionType.ROLLOUT,
                    matcher = new CombiningMatcher
                    {
                        combiner = CombinerEnum.AND,
                        delegates = new List<AttributeMatcher>
                        {
                            new AttributeMatcher
                            {
                                negate = false,
                                matcher = new AllKeysMatcher()
                            }
                        }
                    },
                    partitions = new List<PartitionDefinition>
                    {
                        new PartitionDefinition
                        {
                            size = 100,
                            treatment = treatment
                        }
                    },
                    label = "Default rule"
                };
            }
        }
    }
}
