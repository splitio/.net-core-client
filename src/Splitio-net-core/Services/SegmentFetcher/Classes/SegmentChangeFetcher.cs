using NLog;
using Splitio.Domain;
using Splitio.Services.SegmentFetcher.Interfaces;
using System;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public abstract class SegmentChangeFetcher: ISegmentChangeFetcher
    {
        private SegmentChange segmentChange;
        private static readonly Logger Log = LogManager.GetLogger(typeof(SegmentChangeFetcher).ToString());

        protected abstract SegmentChange FetchFromBackend(string name, long since);

        public SegmentChange Fetch(string name, long since)
        {
            try
            {
                segmentChange = FetchFromBackend(name, since);
            }
            catch(Exception e)
            {
                Log.Error(e, string.Format("Exception caught executing fetch segment changes since={0}", since));
                segmentChange = null; 
            }                   
            return segmentChange;
        }
    }   
}
