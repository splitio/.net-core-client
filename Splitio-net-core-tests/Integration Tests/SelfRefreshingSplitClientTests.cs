using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Client.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Splitio_Tests.Integration_Tests
{
    [Ignore]
    [TestClass]
    public class SelfRefreshingSplitClientTests
    {
        [TestMethod]
        public void GetTreatmentWithConfig_ReturnsSplitResult()
        {
            //Act
            var client = GetClientInstance();
            var splitResult = client.GetTreatmentWithConfig("littlespoon", "always_on");

            //Assert
            Assert.AreEqual("on", splitResult.Treatment);
            Assert.IsNotNull(splitResult.Config);
        }

        [TestMethod]
        public void GetTreatmentWithConfig_WithKeyObject_ReturnsSplitResult()
        {
            //Arange
            var key = new Key("littlespoon", null);

            //Act
            var client = GetClientInstance();
            var splitResult = client.GetTreatmentWithConfig(key, "always_on");

            //Assert
            Assert.AreEqual("on", splitResult.Treatment);
            Assert.IsNotNull(splitResult.Config);
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_ReturnsSplitResult()
        {
            //Arange
            var features = new List<string> { "always_on", "always_off" };

            //Act
            var client = GetClientInstance();
            var splitResults = client.GetTreatmentsWithConfig("littlespoon", features);

            //Assert
            foreach (var result in splitResults)
            {
                if (result.Key.Equals("always_on"))
                {
                    Assert.AreEqual("on", result.Value.Treatment);
                    Assert.IsNotNull(result.Value.Config);
                }
                else
                {
                    Assert.AreEqual("off", result.Value.Treatment);
                    Assert.IsNotNull(result.Value.Config);
                }
            }
        }

        [TestMethod]
        public void GetTreatmentsWithConfig_WithKeyObject_ReturnsSplitResult()
        {
            //Arange
            var key = new Key("littlespoon", null);
            var features = new List<string> { "always_on", "always_off" };

            //Act
            var client = GetClientInstance();
            var splitResults = client.GetTreatmentsWithConfig(key, features);

            //Assert
            foreach (var result in splitResults)
            {
                if (result.Key.Equals("always_on"))
                {
                    Assert.AreEqual("on", result.Value.Treatment);
                    Assert.IsNotNull(result.Value.Config);
                }
                else
                {
                    Assert.AreEqual("off", result.Value.Treatment);
                    Assert.IsNotNull(result.Value.Config);
                }
            }
        }
        
        [TestMethod]
        public void DestroySuccessfully()
        {
            //Act
            var client = GetClientInstance();
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

        private ISplitClient GetClientInstance()
        {
            var apikey = "API_KEY";

            var configurations = new ConfigurationOptions
            {
                FeaturesRefreshRate = 30,
                SegmentsRefreshRate = 30,
                Endpoint = "https://sdk.split-stage.io",
                EventsEndpoint = "https://events.split-stage.io",
                ReadTimeout = 20000,
                ConnectionTimeout = 20000
            };

            var factory = new SplitFactory(apikey, configurations);
            return factory.Client();
        }
    }
}
