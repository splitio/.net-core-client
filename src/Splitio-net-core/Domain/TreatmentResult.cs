namespace Splitio.Domain
{
    public class TreatmentResult
    {
        public string Label { get; set; }
        public string Treatment { get; set; }
        public long? ChangeNumber { get; set; }
        public object Configurations { get; set; }

        public TreatmentResult(string label, string treatment, long? changeNumber, object configurations = null)
        {
            Label = label;
            Treatment = treatment;
            ChangeNumber = changeNumber;
            Configurations = configurations;
        }
    }
}