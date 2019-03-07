using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.InputValidation.Interfaces
{
    public interface ISplitNameValidator
    {
        List<string> SplitNamesAreValid(List<string> splitNames, string method);
        ValidatorResult SplitNameIsValid(string splitName, string method);
    }
}
