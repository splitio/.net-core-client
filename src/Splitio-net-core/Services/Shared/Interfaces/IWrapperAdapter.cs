using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using System.Threading.Tasks;

namespace Splitio.Services.Shared.Interfaces
{
    public interface IWrapperAdapter
    {
        ReadConfigData ReadConfig(ConfigurationOptions config, ILog log);
        Task TaskDelay(int millisecondsDelay);
        Task<T> TaskFromResult<T>(T result);
    }
}
