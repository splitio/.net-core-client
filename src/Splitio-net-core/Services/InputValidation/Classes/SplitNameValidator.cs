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

            var dicSplits = new Dictionary<string, string>();

            foreach (var name in splitNames)
            {
                var splitNameResult = SplitNameIsValid(name, method);

                if (splitNameResult.Success)
                {
                    try { dicSplits.Add(splitNameResult.Value, splitNameResult.Value); } catch { }
                }
            }

            return dicSplits.Keys.ToList();
        }

        public ValidatorResult SplitNameIsValid(string splitName, string method)
        {
            if (splitName == null)
            {
                _log.Error($"{method}: you passed a null split_name, split_name must be a non-empty string");
                return new ValidatorResult { Success = false };
            }

            if (splitName == string.Empty)
            {
                _log.Error($"{method}: you passed an empty split_name, split_name must be a non-empty string");
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
