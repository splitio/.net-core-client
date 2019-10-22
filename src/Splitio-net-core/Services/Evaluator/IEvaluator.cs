using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Evaluator
{
    public interface IEvaluator
    {
        TreatmentResult Evaluate(Key key, string featureName, Dictionary<string, object> attributes = null);
        MultipleEvaluatorResult EvaluateMany(Key key, List<string> featureNames, Dictionary<string, object> attributes = null);        
    }
}
