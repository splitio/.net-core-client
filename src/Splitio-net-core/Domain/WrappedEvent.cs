namespace Splitio.Domain
{
    public class WrappedEvent
    {
        public Event Event { get; set; }
        public long Size { get; set; }
    }
}