#if !NET45
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class IntegrationsTests
    {
        private readonly SplitFactory _splitFactory;

        public IntegrationsTests()
        {
            var apikey = "chirimbolito";
            //var apikey = "mhfvtl3ilt6aeds8i1amn8qjssbdarlr76ju";

            var configurations = new ConfigurationOptions
            {
                FeaturesRefreshRate = 30,
                SegmentsRefreshRate = 30,
                Endpoint = "http://localhost:50286",
                EventsEndpoint = "http://localhost:50286",
                //Endpoint = "https://sdk.split-stage.io",
                //EventsEndpoint = "https://events.split-stage.io",
                ReadTimeout = 20000,
                ConnectionTimeout = 20000,
                ImpressionsRefreshRate = 15
            };

            _splitFactory = new SplitFactory(apikey, configurations);
        }

        [TestMethod]
        public void Test1()
        {
            var httpClientMock = new HttpClientMock(50286);

            httpClientMock.SplitChangesOk("split_changes.json","-1");
            httpClientMock.SplitChangesOk("split_changes_1.json", "1506703262916");

            httpClientMock.SegmentChangesOk("-1", "segment1");
            httpClientMock.SegmentChangesOk("1470947453877", "segment1");

            httpClientMock.SegmentChangesOk("-1", "segment2");
            httpClientMock.SegmentChangesOk("1470947453878", "segment2");

            httpClientMock.SegmentChangesOk("-1", "segment3");
            httpClientMock.SegmentChangesOk("1470947453879", "segment3");

            var client = _splitFactory.Client();
            client.BlockUntilReady(100000);

            httpClientMock.ShutdownServer();
        }
    }
}
#endif