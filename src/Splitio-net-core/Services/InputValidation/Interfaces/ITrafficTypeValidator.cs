using Splitio.Domain;

namespace Splitio.Services.InputValidation.Interfaces
{
    public interface ITrafficTypeValidator
    {
        ValidatorResult IsValid(string trafficType, string method);
    }
}
