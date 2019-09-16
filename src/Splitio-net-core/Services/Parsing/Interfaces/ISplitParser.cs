using Splitio.Domain;

namespace Splitio.Services.Parsing.Interfaces
{
    public interface ISplitParser
    {
        ParsedSplit Parse(Split split);
    }
}
