using System.Net;

namespace Splitio.CommonLibraries
{
    public class HTTPResult
    {
        public HttpStatusCode statusCode { get; set; }
        public string content { get; set; }
    }
}
