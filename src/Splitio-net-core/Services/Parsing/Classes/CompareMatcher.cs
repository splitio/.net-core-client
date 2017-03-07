using Splitio.CommonLibraries;
using Splitio.Domain;
using System;

namespace Splitio.Services.Parsing
{
    public abstract class CompareMatcher:IMatcher
    {
        protected DataTypeEnum? dataType;
        protected long value;
        protected long start;
        protected long end;
        public bool Match(string key)
        {
            switch (dataType)
            {
                case DataTypeEnum.DATETIME:
                    var date = key.ToDateTime();
                    return date != null ? Match(date.Value) : false;
                case DataTypeEnum.NUMBER:
                    long number;
                    var result = long.TryParse(key, out number);
                    return result ? Match(number) : false;
                default: return false;
            }
        }

        public abstract bool Match(DateTime key);

        public abstract bool Match(long key);
    }
}
