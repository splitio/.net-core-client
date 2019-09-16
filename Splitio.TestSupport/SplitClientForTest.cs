using Common.Logging;
using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Client.Classes
{
    public class SplitClientForTest : SplitClient
    {
        private Dictionary<string, string> _tests;

        public SplitClientForTest(ILog log) : base(log)
        {
            _tests = new Dictionary<string, string>();
        }

        public override void Destroy()
        {
            _tests = new Dictionary<string, string>();
        }

        public void ClearTreatments()
        {
            _tests = new Dictionary<string, string>();
        }

        public void RegisterTreatments(Dictionary<string, string> treatments)
        {
            foreach (var treatment in treatments)
            {
                if (!_tests.ContainsKey(treatment.Key))
                {
                    _tests.Add(treatment.Key, treatment.Value);
                }
            }
        }

        public void RegisterTreatment(string feature, string treatment)
        {
            _tests.Add(feature, treatment);
        }

        public string GetTreatment(string key, string feature)
        {
            return _tests.ContainsKey(feature) ? _tests[feature] : "control";
        }

        public override string GetTreatment(string key, string feature, Dictionary<string, object> attributes = null)
        {
            return GetTreatment(key, feature);
        }

        public override string GetTreatment(Key key, string feature, Dictionary<string, object> attributes = null)
        {
            return _tests.ContainsKey(feature) ? _tests[feature] : "control";
        }
    }
}

