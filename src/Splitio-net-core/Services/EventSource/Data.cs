using Newtonsoft.Json;

namespace Splitio.Services.EventSource
{
    public class Data
    {
        public string Id { get; set; }
        public string ConnectionId { get; set; }
        public string Timestamp { get; set; }
        public string Channel { get; set; }
        [JsonProperty("data")]
        public string Content { get; set; }
        public string Name { get; set; }
    }
}
