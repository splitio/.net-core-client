using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitFactoryUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(TimeoutException), "SDK was not ready in 1 miliseconds")]
        public void BuildSplitClientShouldReturnExceptionIfSdkNotReady()
        {
            //Arrange            
            var options = new ConfigurationOptions() { Ready = 1 };
            var factory = new SplitFactory("any", options);

            //Act         
            var client = factory.Client();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "API Key should be set to initialize Split SDK.")]
        public void BuildSplitClientWithEmptyApiKeyShouldReturnException()
        {
            //Arrange
            var factory = new SplitFactory(null, null);

            //Act         
            var client = factory.Client();
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void BuildSplitClientWithLocalhostApiKeyShouldReturnLocalhostClient()
        {
            //Arrange
            var options = new ConfigurationOptions() { LocalhostFilePath = @"Resources\test.splits" };
            var factory = new SplitFactory("localhost", options);

            //Act         
            var client = factory.Client();

            //Assert
            Assert.AreEqual(typeof(LocalhostClient), client.GetType());
        }

        [TestMethod]
        public void BuildSplitClientWithApiKeyShouldReturnSelfRefreshingSplitClient()
        {
            //Arrange
            var factory = new SplitFactory("any", null);

            //Act         
            var client = factory.Client();

            //Assert
            Assert.AreEqual(typeof(SelfRefreshingClient), client.GetType());
        }

        [TestMethod]
        public void BuildSplitClientWithRedisConfigShouldReturnRedisSplitClient()
        {
            //Arrange
            var configurationOptions = new ConfigurationOptions();
            configurationOptions.Mode = Mode.Consumer;
            configurationOptions.CacheAdapterConfig = new CacheAdapterConfigurationOptions();
            configurationOptions.CacheAdapterConfig.Host = "local";
            configurationOptions.CacheAdapterConfig.Port = "1234";
            configurationOptions.CacheAdapterConfig.Password = "test";

            var factory = new SplitFactory("any", configurationOptions);

            //Act         
            var client = factory.Client();

            //Assert
            Assert.AreEqual(typeof(RedisClient), client.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Redis Host, Port and Password should be set to initialize Split SDK in Redis Mode.")]
        public void BuildRedisSplitClientWithoutAllRequiredConfigsShouldReturnException()
        {
            //Arrange
            var configurationOptions = new ConfigurationOptions();
            configurationOptions.Mode = Mode.Consumer;
            configurationOptions.CacheAdapterConfig = new CacheAdapterConfigurationOptions();
            configurationOptions.CacheAdapterConfig.Host = "local";

            var factory = new SplitFactory("any", configurationOptions);

            //Act         
            var client = factory.Client();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Redis config should be set to build split client in Consumer mode.")]
        public void BuildRedisSplitClientAsConsumerWithNullRedisConfigShouldReturnException()
        {
            //Arrange
            var configurationOptions = new ConfigurationOptions();
            configurationOptions.Mode = Mode.Consumer;

            var factory = new SplitFactory("any", configurationOptions);

            //Act         
            var client = factory.Client();

        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Unsupported mode.")]
        public void BuildRedisSplitClientAsProducerShouldReturnException()
        {
            //Arrange
            var configurationOptions = new ConfigurationOptions();
            configurationOptions.Mode = Mode.Producer;

            var factory = new SplitFactory("any", configurationOptions);

            //Act         
            var client = factory.Client();
        }
    }
}
