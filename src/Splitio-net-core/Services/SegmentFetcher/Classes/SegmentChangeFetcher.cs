using Common.Logging;
using Splitio.Domain;
using Splitio.Services.SegmentFetcher.Interfaces;
using System;
using System.Threading.Tasks;

namespace Splitio.Services.SegmentFetcher.Classes
{
    public abstract class SegmentChangeFetcher: ISegmentChangeFetcher
    {
        private SegmentChange segmentChange;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SegmentChangeFetcher));

        protected abstract Task<SegmentChange> FetchFromBackend(string name, long since);

        public async Task<SegmentChange> Fetch(string name, long since)
        {
            try
            {
                segmentChange = await FetchFromBackend(name, since);
            }
            catch(Exception e)
            {
                Log.Error(string.Format("Exception caught executing fetch segment changes since={0}", since), e);
                segmentChange = null; 
            }                   
            return segmentChange;
        }
    }   
}
