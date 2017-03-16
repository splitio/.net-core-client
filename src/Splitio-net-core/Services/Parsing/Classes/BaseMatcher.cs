using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Splitio.Services.Parsing.Classes
{
    public abstract class BaseMatcher
    {
        public abstract bool Match(string key);

        public abstract bool Match(DateTime key);

        public abstract bool Match(long key);

        public bool Match(object value)
        {
            if (value is string)
            {
                return Match((string)value);
            }
            else if (value is DateTime)
            {
                return Match((DateTime)value);
            }
            else if (value is long)
            {
                return Match((long)value);
            }
            else if(value is int)
            {
                return Match((int)value);
            }

            return false;
        }
    }
}
