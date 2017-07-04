using Splitio.Domain;
using Splitio.Services.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Splitio.Services.Parsing.Classes
{
    public abstract class BaseMatcher
    {
        public abstract bool Match(string key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        public abstract bool Match(DateTime key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        public abstract bool Match(long key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        public abstract bool Match(List<string> key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        public abstract bool Match(Key key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);
        
        public abstract bool Match(bool key, Dictionary<string, object> attributes = null, ISplitClient splitClient = null);

        public bool Match(object value, Dictionary<string, object> attributes = null, ISplitClient splitClient = null)
        {
            if (value is string)
            {
                return Match((string)value, attributes, splitClient);
            }
            else if (value is DateTime)
            {
                return Match((DateTime)value, attributes, splitClient);
            }
            else if (value is long)
            {
                return Match((long)value, attributes, splitClient);
            }
            else if(value is int)
            {
                return Match((int)value, attributes, splitClient);
            }
            else if(value is List<string>)
            {
                return Match((List<string>)value, attributes, splitClient);
            }
            else if(value is Key)
            {
                return Match((Key)value, attributes, splitClient);
            }
            else if(value is bool)
            {
                return Match((bool)value, attributes, splitClient);
            }

            return false;
        }
    }
}
