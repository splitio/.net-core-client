using NLog;
using Splitio.Domain;
using Splitio.Services.SplitFetcher.Interfaces;
using System;

namespace Splitio.Services.SplitFetcher.Classes
{
    public abstract class SplitChangeFetcher : ISplitChangeFetcher
    {
        private SplitChangesResult splitChanges;
        private static readonly Logger Log = LogManager.GetLogger(typeof(SplitChangeFetcher).ToString());

        protected abstract SplitChangesResult FetchFromBackend(long since);

        public SplitChangesResult Fetch(long since)
        {
            try
            {
                splitChanges = FetchFromBackend(since);
            }
            catch(Exception e)
            {
                Log.Error(e, string.Format("Exception caught executing Fetch since={0}", since));
                splitChanges = null; 
            }                   
            return splitChanges;
        }
    }
}
