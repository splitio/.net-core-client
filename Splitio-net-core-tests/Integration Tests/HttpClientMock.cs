#if !NET45
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Splitio_Tests.Integration_Tests
{
    public class HttpClientMock
    {
        private readonly FluentMockServer _server;

        public HttpClientMock()
        {
            _server = FluentMockServer.Start(50286);
        }

        public void SplitChangesEndpointMock(int statusCodeExpected)
        {
            var body = statusCodeExpected == 200
                ? @"{ msg: ""Hello world!""}"
                : "";

            _server
                .Given(
                    Request.Create()
                    .WithPath("/api/splitChanges?since=-1")
                    .UsingGet()
                )
                .RespondWith(
                    Response.Create()
                    .WithStatusCode(statusCodeExpected)
                    .WithBody(body));
        }

        public void ShutdownServer()
        {
            _server.Stop();
        }
    }
}
#endif