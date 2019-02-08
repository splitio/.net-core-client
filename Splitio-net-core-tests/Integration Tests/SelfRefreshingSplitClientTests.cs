using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;
using System.Linq;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class SelfRefreshingSplitClientTests
    {
        [TestMethod]
        public void DestroySuccessfully()
        {
            //Arrange
            var apikey = "r6sp9nott1ldiofpj1dc27oqed7dsn7m2vsi";

            var configurations = new ConfigurationOptions();
            configurations.Ready = 60000;
            configurations.FeaturesRefreshRate = 30;
            configurations.SegmentsRefreshRate = 30;
            configurations.Endpoint = "https://sdk-aws-staging.split.io";
            configurations.EventsEndpoint = "https://events-aws-staging.split.io";
            configurations.ReadTimeout = 20000;
            configurations.ConnectionTimeout = 20000;

            var factory = new SplitFactory(apikey, configurations);
            var client = factory.Client();

            //Act
            var result1 = client.GetTreatment("littlespoon", "always_on");

            client.Destroy();

            var resultDestroy1 = client.GetTreatment("littlespoon", "NET_isBetweenNumberWithAttributeValueThatMatches");
            var manager = client.GetSplitManager();
            var resultDestroy2 = manager.Splits();
            var resultDestroy3 = manager.SplitNames();
            var resultDestroy4 = manager.Split("NET_isBetweenNumberWithAttributeValueThatMatches");

            //Assert
            Assert.AreEqual("on", result1);
            Assert.AreEqual("control", resultDestroy1);
            Assert.IsFalse(resultDestroy2.Any());
            Assert.IsFalse(resultDestroy3.Any());
            Assert.IsNull(resultDestroy4);
        }
    }
}
