namespace Splitio.Domain
{
    public class EventValidatorResult
    {
        public bool Success { get; set; }
        public object Value { get; set; }
        public long EventSize { get; set; }
    }
}
