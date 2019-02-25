using Common.Logging;
using Splitio.Domain;
using Splitio.Services.InputValidation.Interfaces;
using System.Linq;

namespace Splitio.Services.InputValidation.Classes
{
    public class TrafficTypeValidator : ITrafficTypeValidator
    {
        protected readonly ILog _log;

        public TrafficTypeValidator(ILog log)
        {
            _log = log;
        }

        public ValidatorResult IsValid(string trafficType, string method)
        {
            if (trafficType == null)
            {
                _log.Error($"{method}: you passed a null or undefined {trafficType}, {trafficType} must be a non-empty string");
                return new ValidatorResult { Success = false };
            }

            if (trafficType == string.Empty)
            {
                _log.Error($"{method}: you passed an empty {trafficType}, {trafficType} must be a non-empty string");
                return new ValidatorResult { Success = false };
            }

            if (trafficType.Any(ch => char.IsUpper(ch)))
            {
                _log.Warn($"{method}: {trafficType} should be all lowercase - converting string to lowercase");

                trafficType = trafficType.ToLower();
            }

            return new ValidatorResult { Success = true, Value = trafficType };
        }
    }
}
