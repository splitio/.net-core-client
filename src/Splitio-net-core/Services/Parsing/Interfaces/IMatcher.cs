using Splitio.Domain;
using Splitio.Services.Client.Interfaces;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public interface IMatcher
    {
        bool Match(object value, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        bool Match(string key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        bool Match(Key key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        bool Match(DateTime key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        bool Match(long key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        bool Match(List<string> key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);
    }
}
