using System.Collections.Generic;

namespace Splitio_net_core.Integration_tests.Resources
{
    public class EventBackend
    {
        public string Key { get; set; }
        public string EventTypeId { get; set; }
        public string TrafficTypeName { get; set; }
        public double? Value { get; set; }
        public long Timestamp { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class EventRedis
    {
        public MachineRedis M { get; set; }
        public EventBackend E { get; set; }
    }
}
