using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Redis.Services.Cache.Classes;
using Splitio.Redis.Services.Cache.Interfaces;
using Splitio.Redis.Services.Client.Classes;
using Splitio.Services.Client.Classes;
using Splitio_Tests.Resources;
using System.Collections.Generic;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class RedisClientTests
    {
        private const string HOST = "localhost";
        private const string PORT = "6379";
        private const string PASSWORD = "";
        private const int DB = 0;

        private ConfigurationOptions config;
        private Mock<ILog> _logMock = new Mock<ILog>();
        private IRedisAdapter _redisAdapter;

        [TestInitialize]
        public void Initialization()
        {
            var cacheAdapterConfig = new CacheAdapterConfigurationOptions
            {
                Host = HOST,
                Port = PORT,
                Password = PASSWORD,
                Database = DB
            };

            config = new ConfigurationOptions();
            config.CacheAdapterConfig = cacheAdapterConfig;
            config.SdkMachineIP = "192.168.0.1";

            _redisAdapter = new RedisAdapter(HOST, PORT, PASSWORD, DB);
            LoadSplits();
        }

        [TestMethod]
        public void GetTreatment_WhenFeatureExists_ReturnsOn()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var result = client.GetTreatment("test", "always_on", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        [TestMethod]
        public void GetTreatment_WhenFeatureExists_ReturnsOff()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var result = client.GetTreatment("test", "always_off", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }

        [TestMethod]
        public void GetTreatment_WhenFeatureDoenstExist_ReturnsControl()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var result = client.GetTreatment("test", "always_control", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result);
        }

        [TestMethod]
        public void GetTreatments_WhenFeaturesExists_ReturnsOnOff()
        {
            //Arrange
            var alwaysOn = "always_on";
            var alwaysOff = "always_off";

            var features = new List<string> { alwaysOn, alwaysOff };

            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var result = client.GetTreatments("test", features, null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result[alwaysOff]);
            Assert.AreEqual("on", result[alwaysOn]);
        }

        [TestMethod]
        public void GetTreatments_WhenOneFeatureDoenstExist_ReturnsOnOffControl()
        {
            //Arrange
            var alwaysOn = "always_on";
            var alwaysOff = "always_off";
            var alwaysControl = "always_control";

            var features = new List<string> { alwaysOn, alwaysOff, alwaysControl };

            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var result = client.GetTreatments("test", features, null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result[alwaysOff]);
            Assert.AreEqual("on", result[alwaysOn]);
            Assert.AreEqual("control", result[alwaysControl]);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnInexistentSplitShouldReturnControl()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var result = client.GetTreatment("test", "fail", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithSegmentShouldReturnOnIfExistent()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            //feature test_jw2 has UserDefinedSegmentMatcher 
            //on "payed" segment, and it has abcdz.
            var result = client.GetTreatment("abcdz", "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithSegmentShouldReturnOffIfNotExistent()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            //feature test_jw2 has UserDefinedSegmentMatcher 
            //on "payed" segment, and it doesn't include abXdz.
            //Default treatment is off
            var result = client.GetTreatment("abXdz", "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnKilledSplitReturnsDefaultTreatment()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act    
            //Default treatment is off
            var result = client.GetTreatment("test", "test_jw3", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentAndUsingBucketingKeyForTreatment()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var key = new Key("abcdz", "2f726442-abbd-43f0-8373-ada456cff612");
            var result = client.GetTreatment(key, "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentAndEmptyBucketingKeyShouldReturnControl()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            var key = new Key("abcdz", "");
            var result = client.GetTreatment(key, "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnRemovedUserFromSegmentShouldReturnOff()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);
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

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithDateMatcherReturnOn()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("date", "1470960003600");
            var result = client.GetTreatment("abcdz", "test_jw", data);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithDateMatcherReturnOff()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("date", "1370960003600");
            var result = client.GetTreatment("abcdz", "test_jw", data);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }

        [Ignore]
        [TestMethod]
        public void ExecuteGetTreatmentOnSplitWithWhitelistMatcherReturnOn()
        {
            //Arrange
            var client = new RedisClient(config, _logMock.Object);

            //Act           
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("date", "1370960003600");
            var result = client.GetTreatment("abcdef", "test_jw4", data);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
        }

        private void LoadSplits()
        {
            _redisAdapter.Flush();

            _redisAdapter.Set("SPLITIO.split.always_on", SplitsHelper.AlwaysOn);
            _redisAdapter.Set("SPLITIO.split.always_off", SplitsHelper.AlwaysOff);
        }
    }
}
