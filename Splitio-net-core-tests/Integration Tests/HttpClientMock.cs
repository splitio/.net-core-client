#if !NET45
using Splitio_Tests.Resources;
using System.IO;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Splitio_Tests.Integration_Tests
{
    public class HttpClientMock
    {
        private readonly FluentMockServer _mockServer;

        public HttpClientMock(int port)
        {
            _mockServer = FluentMockServer.Start(port);
        }

        #region SplitChanges
        public void SplitChangesOk(string fileName, string since)
        {
            string json = File.ReadAllText($"Resources\\{fileName}");

            _mockServer
                .Given(
                    Request.Create()
                    .WithPath("/api/splitChanges")
                    .WithParam("since", since)
                    .UsingGet()
                )
                .RespondWith(
                    Response.Create()
                    .WithStatusCode(200)
                    .WithBody(json));
        }

        public void SplitChangesError(StatusCodeEnum statusCode)
        {
            var body = string.Empty;

            switch (statusCode)
            {
                case StatusCodeEnum.BadRequest:
                    body = "Bad Request";
                    break;
                case StatusCodeEnum.InternalServerError:
                    body = "Internal Server Error";
                    break;
            }

            _mockServer
                .Given(
                    Request.Create()
                    .WithPath("/api/splitChanges*")
                    .UsingGet()
                )
                .RespondWith(
                    Response.Create()
                    .WithStatusCode((int)statusCode)
                    .WithBody(body));
        }
        #endregion

        #region SegmentChanges
        public void SegmentChangesOk(string since, string segmentName)
        {
            string json = File.ReadAllText($"Resources\\split_{segmentName}.json");

            _mockServer
                .Given(
                    Request.Create()
                    .WithPath($"/api/segmentChanges/{segmentName}")
                    .WithParam("since", since)
                    .UsingGet()
                )
                .RespondWith(
                    Response.Create()
                    .WithStatusCode(200)
                    .WithBody(json));
        }

        public void SegmentChangesError(StatusCodeEnum statusCode)
        {
            var body = string.Empty;

            switch (statusCode)
            {
                case StatusCodeEnum.BadRequest:
                    body = "Bad Request";
                    break;
                case StatusCodeEnum.InternalServerError:
                    body = "Internal Server Error";
                    break;
            }

            _mockServer
                .Given(
                    Request.Create()
                    .WithPath($"/api/segmentChanges*")
                    .UsingGet()
                )
                .RespondWith(
                    Response.Create()
                    .WithStatusCode((int)statusCode)
                    .WithBody(body));
        }
        #endregion

        public void ShutdownServer()
        {
            _mockServer.Stop();
        }
    }
}
#endif