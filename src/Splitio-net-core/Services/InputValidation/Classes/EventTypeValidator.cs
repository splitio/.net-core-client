using Common.Logging;
using Splitio.Services.InputValidation.Interfaces;
using System.Text.RegularExpressions;

namespace Splitio.Services.InputValidation.Classes
{
    public class EventTypeValidator : IEventTypeValidator
    {
        private const string REGEX = "^[a-zA-Z0-9][-_.:a-zA-Z0-9]{0,79}$";

        protected readonly ILog _log;

        public EventTypeValidator(ILog log)
        {
            _log = log;
        }

        public bool IsValid(string eventType, string method)
        {
            if (eventType == string.Empty)
            {
                _log.Error($"{method}: you passed an empty {eventType}, {eventType} must be a non-empty String");
                return false;
            }

            if (eventType == null)
            {
                _log.Error($"{method}: you passed a null or undefined {eventType}, {eventType} must be a non-empty String");
                return false;
            }

            if (!Regex.Match(eventType, REGEX).Success)
            {
                _log.Error($"{method}: you passed {eventType}, event name must adhere to the regular expression {REGEX}. This means an event name must be alphanumeric, cannot be more than 80 characters long, and can only include a dash, underscore, period, or colon as separators of alphanumeric characters");
                return false;
            }

            return true;
        }
    }
}
