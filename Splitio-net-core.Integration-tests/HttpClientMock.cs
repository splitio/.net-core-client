using Splitio_Tests.Resources;
using System.IO;
using System.Linq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Splitio_net_core.Integration_tests
{
    public class HttpClientMock
    {
        private readonly FluentMockServer _mockServer;
        private readonly string rootFilePath;

        public HttpClientMock()
        {
            rootFilePath = string.Empty;

#if NETCORE
            rootFilePath = @"Resources\";
#endif

            _mockServer = FluentMockServer.Start();
        }

        #region SplitChanges        
        public void SplitChangesOk(string fileName, string since)
        {
            string jsonBody = File.ReadAllText($"{rootFilePath}{fileName}");

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
                    .WithBody(jsonBody));
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
            string json = File.ReadAllText($"{rootFilePath}split_{segmentName}.json");

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

        public int GetPort()
        {
            return _mockServer.Ports.First();
        }
    }
}
