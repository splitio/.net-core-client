using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Redis.Services.Client.Classes;
using Splitio.Services.Client.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class SplitFactoryUnitTests
    {
        private readonly string rootFilePath;

        public SplitFactoryUnitTests()
        {
            // This line is to clean the warnings.
            rootFilePath = string.Empty;

#if NETCORE
            rootFilePath = @"Resources\";
#endif
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException), "SDK was not ready in 1 miliseconds")]
        public void BuildSplitClientShouldReturnClientDestroyed()
        {
            //Arrange            
            var options = new ConfigurationOptions();
            var factory = new SplitFactory("any", options);

            //Act         
            var client = factory.Client();
            client.BlockUntilReady(1000);
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
            var options = new ConfigurationOptions { LocalhostFilePath = $"{rootFilePath}test.splits" };
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
            var configurationOptions = new ConfigurationOptions
            {
                Mode = Mode.Consumer,
                CacheAdapterConfig = new CacheAdapterConfigurationOptions
                {
                    Host = "localhost",
                    Port = "6379",
                    Password = "",
                    UserPrefix = "build_client_test"
                }
            };

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
            var configurationOptions = new ConfigurationOptions
            {
                Mode = Mode.Consumer,
                CacheAdapterConfig = new CacheAdapterConfigurationOptions { Host = "local" }
            };

            var factory = new SplitFactory("any", configurationOptions);

            //Act         
            var client = factory.Client();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Redis config should be set to build split client in Consumer mode.")]
        public void BuildRedisSplitClientAsConsumerWithNullRedisConfigShouldReturnException()
        {
            //Arrange
            var configurationOptions = new ConfigurationOptions
            {
                Mode = Mode.Consumer
            };

            var factory = new SplitFactory("any", configurationOptions);

            //Act         
            var client = factory.Client();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Unsupported mode.")]
        public void BuildRedisSplitClientAsProducerShouldReturnException()
        {
            //Arrange
            var configurationOptions = new ConfigurationOptions
            {
                Mode = Mode.Producer
            };

            var factory = new SplitFactory("any", configurationOptions);

            //Act         
            var client = factory.Client();
        }

        [TestMethod]
        [DeploymentItem(@"Resources\split.yaml")]
        public void BuildSplitClient_WithLocalhostApiKeyAndIsYamlFile_ReturnsLocalhostClient()
        {
            // Arrange.
            var splitNamesExpected = new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config",
            };

            var configurationOptions = new ConfigurationOptions
            {
                LocalhostFilePath = $"{rootFilePath}split.yaml"
            };

            var factory = new SplitFactory("localhost", configurationOptions);
            var manager = factory.Manager();
            manager.BlockUntilReady(1000);

            // Act.
            var splitsResult = manager.Splits();

            // Assert.
            Assert.AreEqual(splitNamesExpected.Count, splitsResult.Count);
            foreach (var splitName in splitNamesExpected)
            {
                var split = splitsResult.FirstOrDefault(sr => sr.name.Equals(splitName));
                Assert.IsNotNull(split);

                var splitResult = manager.Split(splitName);
                Assert.IsNotNull(splitResult);
                Assert.IsFalse(splitResult.killed);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\split.yml")]
        public void BuildSplitClient_WithLocalhostApiKeyAndIsYmlFile_ReturnsLocalhostClient()
        {
            // Arrange.
            var splitNamesExpected = new List<string>
            {
                "testing_split_on",
                "testing_split_only_wl",
                "testing_split_with_wl",
                "testing_split_off_with_config",
            };

            var configurationOptions = new ConfigurationOptions
            {
                LocalhostFilePath = $"{rootFilePath}split.yml"
            };

            var factory = new SplitFactory("localhost", configurationOptions);
            var manager = factory.Manager();
            manager.BlockUntilReady(1000);

            // Act.
            var splitsResult = manager.Splits();

            // Assert.
            Assert.AreEqual(splitNamesExpected.Count, splitsResult.Count);
            foreach (var splitName in splitNamesExpected)
            {
                var split = splitsResult.FirstOrDefault(sr => sr.name.Equals(splitName));
                Assert.IsNotNull(split);

                var splitResult = manager.Split(splitName);
                Assert.IsNotNull(splitResult);
                Assert.IsFalse(splitResult.killed);
            }
        }
    }
}