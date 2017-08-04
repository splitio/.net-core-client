using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Client.Classes;
using System.Collections.Generic;
using Splitio.Domain;
using Splitio.Redis.Services.Client.Classes;
using Splitio.Redis.Services.Cache.Classes;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    [Ignore]
    public class RedisClientTests
    {
        ConfigurationOptions config;

        [TestInitialize]
        public void Initialization()
        {
            config = new ConfigurationOptions();
            config.CacheAdapterConfig = new CacheAdapterConfigurationOptions();
            config.CacheAdapterConfig.Host = "localhost";
            config.CacheAdapterConfig.Port = "6379";
            config.CacheAdapterConfig.Password = "";
            config.SdkMachineIP = "192.168.0.1";
        }

        [TestMethod]
        public void ExecuteGetTreatmentOnInexistentSplitShouldReturnControl()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            var result = client.GetTreatment("test", "fail", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result);
        }

        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithSegmentShouldReturnOnIfExistent()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            //feature test_jw2 has UserDefinedSegmentMatcher 
            //on "payed" segment, and it has abcdz.
            var result = client.GetTreatment("abcdz", "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithSegmentShouldReturnOffIfNotExistent()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            //feature test_jw2 has UserDefinedSegmentMatcher 
            //on "payed" segment, and it doesn't include abXdz.
            //Default treatment is off
            var result = client.GetTreatment("abXdz", "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }


        [TestMethod]
        public void ExecuteGetTreatmentOnKilledSplitReturnsDefaultTreatment()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act    
            //Default treatment is off
            var result = client.GetTreatment("test", "test_jw3", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }

        [TestMethod]
        public void ExecuteGetTreatmentAndUsingBucketingKeyForTreatment()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            var key = new Key("abcdz", "2f726442-abbd-43f0-8373-ada456cff612");
            var result = client.GetTreatment(key, "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        [TestMethod]
        public void ExecuteGetTreatmentAndEmptyBucketingKeyShouldReturnControl()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            var key = new Key("abcdz", "");
            var result = client.GetTreatment(key, "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result);
        }

        [TestMethod]
        public void ExecuteGetTreatmentOnRemovedUserFromSegmentShouldReturnOff()
        {
            //Arrange
            var client = new RedisClient(config);
            var cache = new RedisSegmentCache(new RedisAdapter("localhost", "6379", ""));
            
            //Act           
            var result = client.GetTreatment("c1321b21-0f70-449f-8979-b8faed67d210", "test_jw2", null);
            cache.RemoveFromSegment("payed", new List<string>() { "c1321b21-0f70-449f-8979-b8faed67d210" });
            var result2 = client.GetTreatment("c1321b21-0f70-449f-8979-b8faed67d210", "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
            Assert.IsNotNull(result2);
            Assert.AreEqual("off", result2);

            //Reset Status
            cache.AddToSegment("payed", new List<string>() { "c1321b21-0f70-449f-8979-b8faed67d210" });
            cache = null;
        }


        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithDateMatcherReturnOn()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("date", "1470960003600");
            var result = client.GetTreatment("abcdz", "test_jw", data);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithDateMatcherReturnOff()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("date", "1370960003600");
            var result = client.GetTreatment("abcdz", "test_jw", data);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }

        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithWhitelistMatcherReturnOn()
        {
            //Arrange
            var client = new RedisClient(config);

            //Act           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("date", "1370960003600");
            var result = client.GetTreatment("abcdef", "test_jw4", data);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }
    }
}
