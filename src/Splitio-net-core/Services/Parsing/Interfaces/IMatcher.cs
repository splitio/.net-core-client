using System;

namespace Splitio.Services.Parsing
{
    public interface IMatcher
    {
        bool Match(string key);

        bool Match(DateTime key);

        bool Match(long key);
    }
}
