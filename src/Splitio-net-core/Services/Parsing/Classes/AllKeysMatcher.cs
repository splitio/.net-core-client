using Splitio.Services.Parsing.Classes;
using System;

namespace Splitio.Services.Parsing
{
    public class AllKeysMatcher : BaseMatcher, IMatcher
    {
        public override bool Match(string key)
        {
            return key != null;
        }

        public override bool Match(DateTime key)
        {
            return key != null;
        }

        public override bool Match(long key)
        {
            return key != null;
        }
    }
}
