using Newtonsoft.Json;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Events.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using System.Collections.Generic;
using System.Net;

namespace Splitio.Services.Events.Classes
{
    public class EventSdkApiClient : SdkApiClient, IEventSdkApiClient
    {
        private const string EventsUrlTemplate = "/api/events/bulk";
        
        private static readonly ISplitLogger Log = WrapperAdapter.GetLogger(typeof(EventSdkApiClient));

        public EventSdkApiClient(HTTPHeader header, string baseUrl, long connectionTimeOut, long readTimeout) 
            : base(header, baseUrl, connectionTimeOut, readTimeout)
        { }

        public async void SendBulkEvents(List<Event> events)
        {
            var eventsJson = JsonConvert.SerializeObject(events, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var response = await ExecutePost(EventsUrlTemplate, eventsJson);

            if ((int)response.statusCode < (int)HttpStatusCode.OK || (int)response.statusCode >= (int)HttpStatusCode.Ambiguous)
            {
                Log.Error(string.Format("Http status executing SendBulkEvents: {0} - {1}", response.statusCode.ToString(), response.content));
            }
        }
    }
}
