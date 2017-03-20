using log4net;
using Splitio.Domain;
using Splitio.Services.SplitFetcher.Interfaces;
using System;

namespace Splitio.Services.SplitFetcher.Classes
{
    public abstract class SplitChangeFetcher : ISplitChangeFetcher
    {
        private SplitChangesResult splitChanges;
        private static readonly ILog Log = LogManager.GetLogger("splitio",typeof(SplitChangeFetcher));

        protected abstract SplitChangesResult FetchFromBackend(long since);

        public SplitChangesResult Fetch(long since)
        {
            try
            {
                splitChanges = FetchFromBackend(since);
            }
            catch(Exception e)
            {
                Log.Error(string.Format("Exception caught executing Fetch since={0}", since), e);
                splitChanges = null; 
            }                   
            return splitChanges;
        }
    }
}
