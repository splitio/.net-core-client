using System.Collections.Generic;

namespace Splitio.Domain
{
    public class MultipleEvaluatorResult
    {
        public Dictionary<string, TreatmentResult> TreatmentResults { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
