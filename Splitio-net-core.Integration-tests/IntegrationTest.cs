using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;
using Splitio_net_core.Integration_tests.Resources;

namespace Splitio_net_core.Integration_tests
{
    //This class is just to test the 'http client mock' implementation.
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        [DeploymentItem(@"Resources\split_changes.json")]
        [DeploymentItem(@"Resources\split_changes_1.json")]
        [DeploymentItem(@"Resources\split_segment1.json")]
        [DeploymentItem(@"Resources\split_segment2.json")]
        [DeploymentItem(@"Resources\split_segment3.json")]
        public void Test_WhenSdkIsReady()
        {
            // Arrange. 
            var httpClientMock = new HttpClientMock();

            httpClientMock.SplitChangesOk("split_changes.json", "-1");
            httpClientMock.SplitChangesOk("split_changes_1.json", "1506703262916");

            httpClientMock.SegmentChangesOk("-1", "segment1");
            httpClientMock.SegmentChangesOk("1470947453877", "segment1");

            httpClientMock.SegmentChangesOk("-1", "segment2");
            httpClientMock.SegmentChangesOk("1470947453878", "segment2");

            httpClientMock.SegmentChangesOk("-1", "segment3");
            httpClientMock.SegmentChangesOk("1470947453879", "segment3");

            var apikey = "apikey1";

            var port = httpClientMock.GetPort();

            var configurations = new ConfigurationOptions
            {
                FeaturesRefreshRate = 30,
                SegmentsRefreshRate = 30,
                Endpoint = $"http://localhost:{port}",
                EventsEndpoint = $"http://localhost:{port}",
                ReadTimeout = 20000,
                ConnectionTimeout = 20000,
                ImpressionsRefreshRate = 15
            };

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            client.BlockUntilReady(10000);            

            var manager = client.GetSplitManager();

            // Act.
            var treatment = client.GetTreatment("mauro", "FACUNDO_TEST");
            var splits = manager.SplitNames();

            // Assert.
            Assert.AreEqual("off", treatment);
            Assert.AreEqual(29, splits.Count);

            httpClientMock.ShutdownServer();
        }

        [TestMethod]
        public void Test_WhenSdkIsnotReady_SplitChangesReturn400()
        {
            // Arrange. 
            var httpClientMock = new HttpClientMock();

            httpClientMock.SplitChangesError(StatusCodeEnum.BadRequest);
            
            var apikey = "apikey2";

            var port = httpClientMock.GetPort();

            var configurations = new ConfigurationOptions
            {
                FeaturesRefreshRate = 30,
                SegmentsRefreshRate = 30,
                Endpoint = $"http://localhost:{port}",
                EventsEndpoint = $"http://localhost:{port}",
                ReadTimeout = 20000,
                ConnectionTimeout = 20000,
                ImpressionsRefreshRate = 15
            };

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            } catch { }

            var manager = client.GetSplitManager();

            // Act.
            var treatment = client.GetTreatment("mauro", "FACUNDO_TEST");
            var splits = manager.SplitNames();

            // Assert.
            Assert.AreEqual("control", treatment);

            httpClientMock.ShutdownServer();
        }

        [TestMethod]
        public void Test_WhenSdkIsnotReady_SegmentChangesReturn500()
        {
            // Arrange. 
            var httpClientMock = new HttpClientMock();

            httpClientMock.SegmentChangesError(StatusCodeEnum.InternalServerError);

            var apikey = "apikey3";

            var port = httpClientMock.GetPort();

            var configurations = new ConfigurationOptions
            {
                FeaturesRefreshRate = 30,
                SegmentsRefreshRate = 30,
                Endpoint = $"http://localhost:{port}",
                EventsEndpoint = $"http://localhost:{port}",
                ReadTimeout = 20000,
                ConnectionTimeout = 20000,
                ImpressionsRefreshRate = 15
            };

            var splitFactory = new SplitFactory(apikey, configurations);
            var client = splitFactory.Client();

            try
            {
                client.BlockUntilReady(10000);
            }
            catch { }

            var manager = client.GetSplitManager();

            // Act.
            var treatment = client.GetTreatment("mauro", "FACUNDO_TEST");
            var splits = manager.SplitNames();

            // Assert.
            Assert.AreEqual("control", treatment);

            httpClientMock.ShutdownServer();
        }
    }
}
