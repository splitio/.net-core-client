using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Shared.Classes;

namespace Splitio.Services.Shared.Interfaces
{
    public interface IConfigService
    {
        BaseConfig ReadConfig(ConfigurationOptions config, ConfingTypes confingType);
    }
}
