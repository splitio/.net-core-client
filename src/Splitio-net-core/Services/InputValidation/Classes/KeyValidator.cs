using Common.Logging;
using Splitio.Domain;
using Splitio.Services.InputValidation.Interfaces;

namespace Splitio.Services.InputValidation.Classes
{
    public class KeyValidator : IKeyValidator
    {
        private const int KEY_MAX_LENGTH = 250;

        protected readonly ILog _log;

        public KeyValidator(ILog log)
        {
            _log = log;
        }

        public bool IsValid(Key key, string method)
        {
            var matchingKeyIsValid = Validate(key.matchingKey, method, nameof(key.matchingKey));
            var bucketingKeyIsValid = Validate(key.bucketingKey, method, nameof(key.bucketingKey));

            return matchingKeyIsValid && bucketingKeyIsValid;
        }

        private bool Validate(string key, string method, string type)
        {
            if (key == null)
            {
                _log.Error($"{method}: you passed a null or undefined {type}, the {type} must be a non-empty string.");
                return false;
            }

            if (key == string.Empty)
            {
                _log.Error($"{method}: you passed an empty string, {type} must be a non-empty string.");
                return false;
            }

            if (key.Length > KEY_MAX_LENGTH)
            {
                _log.Error($"{method}: {type} too long - must be 250 characters or less.");
                return false;
            }

            return true;
        }
    }
}
