using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public interface IPushManager
    {
        Task<bool> StartSse();
        void StopSse();
    }
}
