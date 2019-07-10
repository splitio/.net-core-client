namespace Splitio.Services.Shared.Interfaces
{
    public interface IFactoryInstantiationsService
    {
        void Decrease(string apiKey);
        void Increase(string apiKey);
    }
}
