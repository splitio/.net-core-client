using Common.Logging;
using Splitio.Services.InputValidation.Interfaces;

namespace Splitio.Services.InputValidation.Classes
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        protected readonly ILog _log;

        public ApiKeyValidator(ILog log)
        {
            _log = log;
        }

        public void Validate(string apiKey)
        {
            if (apiKey == string.Empty)
            {
                _log.Error("factory instantiation: you passed and empty api_key, api_key must be a non-empty string.");
            }

            if (apiKey == null)
            {
                _log.Error("factory instantiation: you passed a null api_key, api_key must be a non-empty string.");
            }
        }
    }
}
