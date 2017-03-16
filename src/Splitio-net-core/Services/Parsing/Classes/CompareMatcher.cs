using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Parsing.Classes;
using System;

namespace Splitio.Services.Parsing
{
    public abstract class CompareMatcher: BaseMatcher, IMatcher
    {
        protected DataTypeEnum? dataType;
        protected long value;
        protected long start;
        protected long end;
        public override bool Match(string key)
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

        public abstract override bool Match(DateTime key);

        public abstract override bool Match(long key);
    }
}
