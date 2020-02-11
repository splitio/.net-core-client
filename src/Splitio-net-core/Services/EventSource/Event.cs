using Newtonsoft.Json;

namespace Splitio.Services.EventSource
{
    public class Event
    {
        public string Id { get; set; }
        [JsonProperty("event")]
        public string Type { get; set; }
        public Data Data { get; set; }
    }
}
