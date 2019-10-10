using Splitio.Domain;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;

namespace Splitio.Services.InputValidation.Classes
{
    public class KeyValidator : IKeyValidator
    {
        private const int KEY_MAX_LENGTH = 250;

        protected readonly ISplitLogger _log;

        public KeyValidator(ISplitLogger log = null)
        {
            _log = log ?? WrapperAdapter.GetLogger(typeof(KeyValidator));
        }

        public bool IsValid(Key key, string method)
        {
            var matchingKeyIsValid = Validate(key?.matchingKey, method, nameof(key.matchingKey));
            var bucketingKeyIsValid = Validate(key?.bucketingKey, method, nameof(key.bucketingKey));

            return matchingKeyIsValid && bucketingKeyIsValid;
        }

        private bool Validate(string key, string method, string type)
        {
            if (key == null)
            {
                _log.Error($"{method}: you passed a null {type}, the {type} must be a non-empty string.");
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
