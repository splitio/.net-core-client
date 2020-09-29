using Newtonsoft.Json;

namespace Splitio.Domain
{
    public class KeyImpression
    {
        public KeyImpression() { }

        public KeyImpression(string matchingKey, string feature, string treatment, long time, long? changeNumber, string label, string bucketingKey)
        {
            this.feature = feature;
            keyName = matchingKey;
            this.treatment = treatment;
            this.time = time;
            this.changeNumber = changeNumber;
            this.label = label;
            this.bucketingKey = bucketingKey;
        }

        [JsonIgnore]
        public string feature { get; set; }
        public string keyName { get; set; }
        public string treatment { get; set; }
        public long time { get; set; }
        public long? changeNumber { get; set; }
        public string label { get; set; }
        public string bucketingKey { get; set; }
        public long? previousTime { get; set; }
    }
    
}
