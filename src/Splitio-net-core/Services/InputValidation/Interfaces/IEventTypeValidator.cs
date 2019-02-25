namespace Splitio.Services.InputValidation.Interfaces
{
    public interface IEventTypeValidator
    {
        bool IsValid(string eventType, string method);
    }
}
