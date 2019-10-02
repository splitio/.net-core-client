using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Client.Interfaces
{
    public interface ISplitClient
    {
        ISplitManager GetSplitManager();
        string GetTreatment(string key, string feature, Dictionary<string, object> attributes = null);
        string GetTreatment(Key key, string feature, Dictionary<string, object> attributes = null);
        SplitResult GetTreatmentWithConfig(string key, string feature, Dictionary<string, object> attributes = null);
        SplitResult GetTreatmentWithConfig(Key key, string feature, Dictionary<string, object> attributes = null);
        Dictionary<string, string> GetTreatments(string key, List<string> features, Dictionary<string, object> attributes = null);
        Dictionary<string, string> GetTreatments(Key key, List<string> features, Dictionary<string, object> attributes = null);
        Dictionary<string, SplitResult> GetTreatmentsWithConfig(string key, List<string> features, Dictionary<string, object> attributes = null);
        Dictionary<string, SplitResult> GetTreatmentsWithConfig(Key key, List<string> features, Dictionary<string, object> attributes = null);
        bool Track(string key, string trafficType, string eventType, double? value = null, Dictionary<string, object> properties = null);
        void Destroy();
        bool IsDestroyed();
        void BlockUntilReady(int blockMilisecondsUntilReady);
    }
}
