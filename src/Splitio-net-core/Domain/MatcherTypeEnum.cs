
namespace Splitio.Domain
{
    public enum MatcherTypeEnum
    {
        ALL_KEYS,
        IN_SEGMENT,
        WHITELIST,
        EQUAL_TO,
        GREATER_THAN_OR_EQUAL_TO,
        LESS_THAN_OR_EQUAL_TO,
        BETWEEN,
        EQUAL_TO_SET,
        CONTAINS_ANY_OF_SET,
        CONTAINS_ALL_OF_SET,
        PART_OF_SET,
        STARTS_WITH,
        ENDS_WITH,
        CONTAINS_STRING,
        IN_SPLIT_TREATMENT,
        EQUAL_TO_BOOLEAN,
        MATCHES_STRING
    }
}
