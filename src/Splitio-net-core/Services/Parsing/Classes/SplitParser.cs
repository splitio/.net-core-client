using Common.Logging;
using Newtonsoft.Json;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.Parsing
{
    public abstract class SplitParser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SplitParser));
        protected ISegmentCache segmentsCache;

        public ParsedSplit Parse(Split split)
        {
            try
            {
                StatusEnum result;
                var isValidStatus = Enum.TryParse(split.status, out result);
                if (!isValidStatus || result != StatusEnum.ACTIVE)
                {
                    return null;
                }

                ParsedSplit parsedSplit = new ParsedSplit()
                {
                    name = split.name,
                    killed = split.killed,
                    defaultTreatment = split.defaultTreatment,
                    seed = split.seed,
                    conditions = new List<ConditionWithLogic>(),
                    changeNumber = split.changeNumber,
                    trafficTypeName = split.trafficTypeName,
                    algo = split.algo == 0 || split.algo == null ? AlgorithmEnum.LegacyHash : (AlgorithmEnum)split.algo,
                    trafficAllocation = split.trafficAllocation,
                    trafficAllocationSeed = split.trafficAllocationSeed.HasValue ? split.trafficAllocationSeed.Value : 0,
                    configurations = JsonConvert.SerializeObject(split.configurations)
                };
                parsedSplit = ParseConditions(split, parsedSplit);
                return parsedSplit;
            }
            catch (Exception e)
            {
                Log.Error("Exception caught parsing split", e);
                return null;
            }
        }

        private ParsedSplit ParseConditions(Split split, ParsedSplit parsedSplit)
        {
            foreach (var condition in split.conditions)
            {
                ConditionType result;
                var isValidCondition = Enum.TryParse(condition.conditionType, out result);
                parsedSplit.conditions.Add(new ConditionWithLogic()
                {
                    conditionType = isValidCondition ? result : ConditionType.WHITELIST,
                    partitions = condition.partitions,
                    matcher = ParseMatcherGroup(parsedSplit, condition.matcherGroup),
                    label = condition.label
                });
            }
            return parsedSplit;
        }

        private CombiningMatcher ParseMatcherGroup(ParsedSplit parsedSplit, MatcherGroupDefinition matcherGroupDefinition)
        {
            if (matcherGroupDefinition.matchers == null || matcherGroupDefinition.matchers.Count() == 0)
            {
                throw new Exception("Missing or empty matchers");
            }

            return new CombiningMatcher()
            {
                delegates = matcherGroupDefinition.matchers.Select(x => ParseMatcher(parsedSplit, x)).ToList(),
                combiner = ParseCombiner(matcherGroupDefinition.combiner)
            };
        }

        private AttributeMatcher ParseMatcher(ParsedSplit parsedSplit, MatcherDefinition matcherDefinition)
        {
            if (matcherDefinition.matcherType == null)
            {
                throw new Exception("Missing matcher type value");
            }
            var matcherType = matcherDefinition.matcherType;

            IMatcher matcher = null;
            try
            {
                MatcherTypeEnum result;
                var isValidMatcherType = Enum.TryParse(matcherType, out result);
                if (isValidMatcherType)
                {
                    switch (result)
                    {
                        case MatcherTypeEnum.ALL_KEYS:
                            matcher = GetAllKeysMatcher(); break;
                        case MatcherTypeEnum.BETWEEN:
                            matcher = GetBetweenMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.EQUAL_TO:
                            matcher = GetEqualToMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.GREATER_THAN_OR_EQUAL_TO:
                            matcher = GetGreaterThanOrEqualToMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.IN_SEGMENT:
                            matcher = GetInSegmentMatcher(matcherDefinition, parsedSplit); break;
                        case MatcherTypeEnum.LESS_THAN_OR_EQUAL_TO:
                            matcher = GetLessThanOrEqualToMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.WHITELIST:
                            matcher = GetWhitelistMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.EQUAL_TO_SET:
                            matcher = GetEqualToSetMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.CONTAINS_ANY_OF_SET:
                            matcher = GetContainsAnyOfSetMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.CONTAINS_ALL_OF_SET:
                            matcher = GetContainsAllOfSetMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.PART_OF_SET:
                            matcher = GetPartOfSetMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.STARTS_WITH:
                            matcher = GetStartsWithMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.ENDS_WITH:
                            matcher = GetEndsWithMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.CONTAINS_STRING:
                            matcher = GetContainsStringMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.IN_SPLIT_TREATMENT: matcher = GetDependencyMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.EQUAL_TO_BOOLEAN: matcher = GetEqualToBooleanMatcher(matcherDefinition); break;
                        case MatcherTypeEnum.MATCHES_STRING: matcher = GetMatchesStringMatcher(matcherDefinition); break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error parsing matcher", e);
            }

            if (matcher == null)
            {
                throw new Exception(string.Format("Unable to create matcher for matcher type: {0}", matcherType));
            }

            AttributeMatcher attributeMatcher = new AttributeMatcher()
            {
                matcher = matcher,
                negate = matcherDefinition.negate
            };

            if (matcherDefinition.keySelector != null && matcherDefinition.keySelector.attribute != null)
            {
                attributeMatcher.attribute = matcherDefinition.keySelector.attribute;
            }

            return attributeMatcher;
        }

        private IMatcher GetMatchesStringMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.stringMatcherData;
            return new MatchesStringMatcher(matcherData);
        }

        private IMatcher GetEqualToBooleanMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.booleanMatcherData;
            return new EqualToBooleanMatcher(matcherData.Value);
        }


        private IMatcher GetDependencyMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.dependencyMatcherData;
            return new DependencyMatcher(matcherData.split, matcherData.treatments);
        }

        private IMatcher GetBetweenMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.betweenMatcherData;
            return new BetweenMatcher(matcherData.dataType, matcherData.start, matcherData.end);
        }

        private IMatcher GetLessThanOrEqualToMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.unaryNumericMatcherData;
            return new LessOrEqualToMatcher(matcherData.dataType, matcherData.value);
        }

        private IMatcher GetGreaterThanOrEqualToMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.unaryNumericMatcherData;
            return new GreaterOrEqualToMatcher(matcherData.dataType, matcherData.value);
        }

        private IMatcher GetEqualToMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.unaryNumericMatcherData;
            return new EqualToMatcher(matcherData.dataType, matcherData.value);
        }

        private IMatcher GetWhitelistMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new WhitelistMatcher(matcherData.whitelist);
        }

        private IMatcher GetEqualToSetMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new EqualToSetMatcher(matcherData.whitelist);
        }

        private IMatcher GetContainsAnyOfSetMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new ContainsAnyOfSetMatcher(matcherData.whitelist);
        }

        private IMatcher GetContainsStringMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new ContainsStringMatcher(matcherData.whitelist);
        }

        private IMatcher GetEndsWithMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new EndsWithMatcher(matcherData.whitelist);
        }

        private IMatcher GetStartsWithMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new StartsWithMatcher(matcherData.whitelist);
        }

        private IMatcher GetPartOfSetMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new PartOfSetMatcher(matcherData.whitelist);
        }

        private IMatcher GetContainsAllOfSetMatcher(MatcherDefinition matcherDefinition)
        {
            var matcherData = matcherDefinition.whitelistMatcherData;
            return new ContainsAllOfSetMatcher(matcherData.whitelist);
        }

        protected abstract IMatcher GetInSegmentMatcher(MatcherDefinition matcherDefinition, ParsedSplit parsedSplit);

        private IMatcher GetAllKeysMatcher()
        {
            return new AllKeysMatcher();
        }

        private CombinerEnum ParseCombiner(string combinerEnum)
        {
            CombinerEnum result;
            var isValidCombiner = Enum.TryParse(combinerEnum, out result);
            return result;
        }
    }
}
