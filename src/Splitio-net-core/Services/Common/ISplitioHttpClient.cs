using Splitio.CommonLibraries;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public interface ISplitioHttpClient : IDisposable
    {
        Task<HTTPResult> GetAsync(string url);
        Task<HttpResponseMessage> GetAsync(string url, HttpCompletionOption completionOption, CancellationToken cancellationToken);
    }
}
