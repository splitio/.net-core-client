using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Client.Interfaces
{
    public interface ISplitClient
    {
        ISplitManager GetSplitManager();

        string GetTreatment(string key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false);

        string GetTreatment(Key key, string feature, Dictionary<string, object> attributes = null, bool logMetricsAndImpressions = true, bool multiple = false);

        Dictionary<string, string> GetTreatments(string key, List<string> features, Dictionary<string, object> attributes = null);

        Dictionary<string, string> GetTreatments(Key key, List<string> features, Dictionary<string, object> attributes = null);
        bool Track(string key, string trafficType, string eventType, double? value);
        bool Track(string key, string trafficType, string eventType);

        void Destroy();
    }
}
