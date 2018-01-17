namespace Splitio.Domain
{
    public class Event
    {
        public string key { get; set; }
        public string trafficTypeName { get; set; }
        public string eventTypeId { get; set; }
        public double? value { get; set; }
        public long timestamp { get; set; }
    }
}
