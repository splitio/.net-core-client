using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.InputValidation.Interfaces
{
    public interface IEventPropertiesValidator
    {
        EventValidatorResult IsValid(Dictionary<string, object> properties);
    }
}