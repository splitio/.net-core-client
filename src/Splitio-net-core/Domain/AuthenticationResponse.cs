using Newtonsoft.Json;

namespace Splitio.Domain
{
    public class AuthenticationResponse
    {
        public bool? PushEnabled { get; set; }
        public string Token { get; set; }
        public string Channels { get; set; }
        public double? Exp { get; set; }
        public bool? Retry { get; set; }
    }

    public class Jwt
    {
        [JsonProperty("x-ably-capability")]
        public string Capability { get; set; }
        [JsonProperty("x-ably-clientId")]
        public string ClientId { get; set; }
        public long Exp { get; set; }
        public long Iat { get; set; }
    }
}
