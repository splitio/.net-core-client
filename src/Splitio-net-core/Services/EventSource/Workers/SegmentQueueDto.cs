namespace Splitio.Services.EventSource.Workers
{
    public class SegmentQueueDto
    {
        public long ChangeNumber { get; set; }
        public string SegmentName { get; set; }

        public SegmentQueueDto(long changeNumber, string segmentName)
        {
            ChangeNumber = changeNumber;
            SegmentName = segmentName;
        }

        public override string ToString()
        {
            return $"segmentName: {SegmentName} - changeNumber: {ChangeNumber}";
        }
    }
}
