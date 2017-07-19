using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;
using System;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    [Ignore]
    public class SelfRefreshingSplitClientTests
    {
        [TestMethod]
        public void DestroySuccessfully()
        {
            //Arrange
            String apikey = "WRITE_API_KEY_HERE";

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
            var result1 = client.GetTreatment("littlespoon", "NET_isBetweenNumberWithAttributeValueThatMatches");

            client.Destroy();

            var resultDestroy1 = client.GetTreatment("littlespoon", "NET_isBetweenNumberWithAttributeValueThatMatches");
            var manager = client.GetSplitManager();
            var resultDestroy2 = manager.Splits();
            var resultDestroy3 = manager.SplitNames();
            var resultDestroy4 = manager.Split("NET_isBetweenNumberWithAttributeValueThatMatches");

            //Assert
            Assert.IsTrue(result1 == "V1");
            Assert.IsTrue(resultDestroy1 == "control");
            Assert.AreEqual(resultDestroy2.Count, 0);
            Assert.AreEqual(resultDestroy3.Count, 0);
            Assert.IsTrue(resultDestroy4 == null);
        }
    }
}
