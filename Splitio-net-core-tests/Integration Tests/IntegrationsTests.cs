#if !NET45
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class IntegrationsTests
    {
        [TestMethod]
        public void Test1()
        {
            var httpClientMock = new HttpClientMock();

            httpClientMock.SplitChangesEndpointMock(200);

            var apikey = "chirimbolito";

            var configurations = new ConfigurationOptions
            {
                FeaturesRefreshRate = 30,
                SegmentsRefreshRate = 30,
                Endpoint = "http://localhost:50286",
                EventsEndpoint = "http://localhost:50286",
                ReadTimeout = 20000,
                ConnectionTimeout = 20000,
                ImpressionsRefreshRate = 15
            };

            var factory = new SplitFactory(apikey, configurations);

            var client = factory.Client();
            client.BlockUntilReady(100000);

            httpClientMock.ShutdownServer();
        }
    }
}
#endif