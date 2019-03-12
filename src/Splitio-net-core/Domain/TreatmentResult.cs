namespace Splitio.Domain
{
    public class TreatmentResult
    {
        public string Label { get; set; }
        public string Treatment { get; set; }
        public long? ChangeNumber { get; set; }

        public TreatmentResult(string label, string treatment, long? changeNumber)
        {
            Label = label;
            Treatment = treatment;
            ChangeNumber = changeNumber;
        }
    }
}