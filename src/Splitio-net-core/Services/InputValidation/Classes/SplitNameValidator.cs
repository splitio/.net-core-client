using Common.Logging;
using Splitio.Domain;
using Splitio.Services.InputValidation.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio.Services.InputValidation.Classes
{
    public class SplitNameValidator : ISplitNameValidator
    {
        private const string WHITESPACE = " ";

        protected readonly ILog _log;

        public SplitNameValidator(ILog log)
        {
            _log = log;
        }

        public List<string> SplitNamesAreValid(List<string> splitNames, string method)
        {
            if (splitNames == null)
            {
                _log.Error($"{method}: split_names must be a non-empty array");
                return splitNames;
            }

            if (!splitNames.Any())
            {
                _log.Error($"{method}: split_names must be a non-empty array");
                return splitNames;
            }

            var splits = new List<string>();

            foreach (var name in splitNames)
            {
                var splitName = CheckWhiteSpaces(name, method);

                if (!splits.Contains(splitName)) splits.Add(splitName);
            }

            return splits;
        }

        public ValidatorResult SplitNameIsValid(string splitName, string method)
        {
            if (splitName == null)
            {
                _log.Error($"{method}: you passed a null or undefined split name, split name must be a non-empty string");
                return new ValidatorResult { Success = false };
            }

            if (splitName == string.Empty)
            {
                _log.Error($"{method}: you passed an empty split name, split name must be a non-empty string");
                return new ValidatorResult { Success = false };
            }

            splitName = CheckWhiteSpaces(splitName, method);

            return new ValidatorResult { Success = true, Value = splitName };
        }

        private string CheckWhiteSpaces(string splitName, string method)
        {
            if (splitName.StartsWith(WHITESPACE) || splitName.EndsWith(WHITESPACE))
            {
                _log.Warn($"{method}: split name {splitName} has extra whitespace, trimming");

                splitName = splitName.TrimStart();
                splitName = splitName.TrimEnd();
            }

            return splitName;
        }
    }
}
