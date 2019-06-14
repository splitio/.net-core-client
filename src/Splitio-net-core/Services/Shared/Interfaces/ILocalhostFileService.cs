using Splitio.Domain;
using System.Collections.Concurrent;

namespace Splitio.Services.Shared.Interfaces
{
    public interface ILocalhostFileService
    {
        ConcurrentDictionary<string, ParsedSplit> ParseSplitFile(string filePath);
    }
}
