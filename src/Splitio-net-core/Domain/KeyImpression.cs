using Newtonsoft.Json;

namespace Splitio.Domain
{
    public class KeyImpression
    {
        [JsonIgnore]
        public string feature { get; set; }
        public string keyName { get; set; }
        public string treatment { get; set; }
        public long time { get; set; }
        public long? changeNumber { get; set; }
        public string label { get; set; }
        public string bucketingKey { get; set; }
    }
    
}
