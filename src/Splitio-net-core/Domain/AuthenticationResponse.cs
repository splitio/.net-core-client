using Newtonsoft.Json;

namespace Splitio.Domain
{
    public class AuthenticationResponse
    {
        public bool? PushEnabled { get; set; }
        public string Token { get; set; }
        public string Channels { get; set; }
        public double? Expiration { get; set; }
        public bool? Retry { get; set; }
    }

    public class Jwt
    {
        [JsonProperty("x-ably-capability")]
        public string Capability { get; set; }
        [JsonProperty("x-ably-clientId")]
        public string ClientId { get; set; }
        [JsonProperty("exp")]
        public long Expiration { get; set; }
    }
}
