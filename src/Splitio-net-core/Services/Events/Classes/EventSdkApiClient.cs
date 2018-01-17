using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Services.Events.Interfaces;
using System.Net;

namespace Splitio.Services.Events.Classes
{
    public class EventSdkApiClient : SdkApiClient, IEventSdkApiClient
    {
        private const string EventsUrlTemplate = "/api/events/bulk";
        
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventSdkApiClient));

        public EventSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout) : base(header, baseUrl, connectionTimeOut, readTimeout) { }

        public void SendBulkEvents(string events)
        {
            var response = ExecutePost(EventsUrlTemplate, events);
            if (response.statusCode != HttpStatusCode.OK)
            {
                Log.Error(string.Format("Http status executing SendBulkEvents: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }
    }
}
