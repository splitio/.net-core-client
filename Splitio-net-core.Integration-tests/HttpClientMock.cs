using Splitio_net_core.Integration_tests.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Splitio_net_core.Integration_tests
{
    public class HttpClientMock : IDisposable
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
            var jsonBody = File.ReadAllText($"{rootFilePath}{fileName}");

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
            var json = File.ReadAllText($"{rootFilePath}split_{segmentName}.json");

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

        #region SSE
        public void SSE_Channels_Response(string bodyExpected)
        {
            _mockServer
                .Given(
                    Request.Create()
                    .UsingGet()
                )
                .RespondWith(
                    Response.Create()
                    .WithStatusCode(200)
                    .WithBody(bodyExpected));
        }
        #endregion

        public void ShutdownServer()
        {
            _mockServer.Stop();
        }

        public string GetUrl()
        {
            return _mockServer.Urls.FirstOrDefault();
        }

        public int GetPort()
        {
            return _mockServer.Ports.First();
        }

        public List<LogEntry> GetLogs()
        {
            return _mockServer.LogEntries.ToList();
        }

        public List<LogEntry> GetImpressionLogs()
        {
            return _mockServer
                .LogEntries
                .Where(l => l.RequestMessage.AbsolutePath.Contains("api/testImpressions/bulk"))
                .ToList();
        }

        public List<LogEntry> GetEventsLog()
        {
            return _mockServer
                .LogEntries
                .Where(l => l.RequestMessage.AbsolutePath.Contains("api/events/bulk"))
                .ToList();
        }

        public void Dispose()
        {
            ShutdownServer();
        }
    }
}
