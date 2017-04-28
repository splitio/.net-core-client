using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;

namespace Splitio.Services.Parsing
{
    public class WhitelistMatcher: BaseMatcher, IMatcher
    {
        private List<string> list;

        public WhitelistMatcher(List<string> list)
        {
            this.list = list ?? new List<string>();
        }
        public override bool Match(string key)
        {
            return list.Contains(key);
        }

        public override bool Match(DateTime key)
        {
            return false;
        }

        public override bool Match(long key)
        {
            return false;
        }

        public override bool Match(List<string> key)
        {
            return false;
        }
    }
}
