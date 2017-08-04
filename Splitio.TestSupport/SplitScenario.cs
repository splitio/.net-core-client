using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Splitio.TestSupport
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SplitScenario : ClassDataAttribute
    {
        public SplitTest[] features { get; private set; }

        [JsonConstructor]
        public SplitScenario(SplitTest[] features) : base(typeof(object))
        {
            this.features = features;
        }

        public SplitScenario(string features) : base(typeof(object))
        {
            this.features = JsonConvert.DeserializeObject<SplitTest[]>(features);
        }

        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest)
        {
            var data = new List<string[]>();
            foreach (var splitTest in this.features)
            {
                foreach (var treatment in splitTest.treatments)
                {
                    data.Add(new string[] { splitTest.feature, treatment });
                }
            }
            return data;
        }
    }
}
