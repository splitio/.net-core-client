using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_Tests.Unit_Tests.Shared
{
    [TestClass]
    public class ConfigServiceTests
    {
        private readonly Mock<IWrapperAdapter> _wrapperAdapter;
        private readonly Mock<ISplitLogger> _log;

        private readonly IConfigService _configService;

        public ConfigServiceTests()
        {
            _wrapperAdapter = new Mock<IWrapperAdapter>();
            _log = new Mock<ISplitLogger>();

            _configService = new ConfigService(_wrapperAdapter.Object, _log.Object);
        }

        [TestMethod]
        public void GetInMemoryDefatulConfig()
        {
            _wrapperAdapter
                .Setup(mock => mock.ReadConfig(It.IsAny<ConfigurationOptions>(), It.IsAny<ISplitLogger>()))
                .Returns(new ReadConfigData
                {
                    SdkMachineIP = "ip-test",
                    SdkMachineName = "name-test",
                    SdkSpecVersion = "version-test",
                    SdkVersion = "version-test",
                });

            var result = (SelfRefreshingConfig)_configService.ReadConfig(new ConfigurationOptions(), ConfingTypes.InMemory);

            Assert.AreEqual(true, result.LabelsEnabled);
            Assert.AreEqual("https://sdk.split.io", result.BaseUrl);
            Assert.AreEqual("https://events.split.io", result.EventsBaseUrl);
            Assert.AreEqual(5, result.SplitsRefreshRate);
            Assert.AreEqual(60, result.SegmentRefreshRate);
            Assert.AreEqual(15000, result.HttpConnectionTimeout);
            Assert.AreEqual(15000, result.HttpReadTimeout);
            Assert.AreEqual(false, result.RandomizeRefreshRates);
            Assert.AreEqual(30, result.TreatmentLogRefreshRate);
            Assert.AreEqual(30000, result.TreatmentLogSize);
            Assert.AreEqual(60, result.EventLogRefreshRate);
            Assert.AreEqual(5000, result.EventLogSize);
            Assert.AreEqual(10, result.EventsFirstPushWindow);
            Assert.AreEqual(1000, result.MaxCountCalls);
            Assert.AreEqual(60, result.MaxTimeBetweenCalls);
            Assert.AreEqual(5, result.NumberOfParalellSegmentTasks);
            Assert.AreEqual(true, result.StreamingEnabled);
            Assert.AreEqual(1, result.AuthRetryBackoffBase);
            Assert.AreEqual(1, result.StreamingReconnectBackoffBase);
            Assert.AreEqual("https://auth.split.io/api/auth", result.AuthServiceURL);
            Assert.AreEqual("https://streaming.split.io/event-stream", result.StreamingServiceURL);
            Assert.AreEqual(ImpressionModes.Optimized, result.ImpressionMode);
            Assert.AreEqual("ip-test", result.SdkMachineIP);
            Assert.AreEqual("name-test", result.SdkMachineName);
            Assert.AreEqual("version-test", result.SdkSpecVersion);
            Assert.AreEqual("version-test", result.SdkVersion);
            Assert.AreEqual(true, result.LabelsEnabled);
        }

        [TestMethod]
        public void GetInMemoryCustomConfig()
        {
            // Arrange.
            _wrapperAdapter
                .Setup(mock => mock.ReadConfig(It.IsAny<ConfigurationOptions>(), It.IsAny<ISplitLogger>()))
                .Returns(new ReadConfigData
                {
                    SdkMachineIP = "ip-test",
                    SdkMachineName = "name-test",
                    SdkSpecVersion = "version-test",
                    SdkVersion = "version-test",
                });

            var config = new ConfigurationOptions
            {
                ImpressionMode = ImpressionModes.Debug,
                FeaturesRefreshRate = 100, 
                ImpressionsRefreshRate = 150,
                SegmentsRefreshRate = 80,
                StreamingEnabled = false
            };

            // Act.
            var result = (SelfRefreshingConfig)_configService.ReadConfig(config, ConfingTypes.InMemory);

            // Assert.
            Assert.AreEqual(true, result.LabelsEnabled);
            Assert.AreEqual("https://sdk.split.io", result.BaseUrl);
            Assert.AreEqual("https://events.split.io", result.EventsBaseUrl);
            Assert.AreEqual(100, result.SplitsRefreshRate);
            Assert.AreEqual(80, result.SegmentRefreshRate);
            Assert.AreEqual(15000, result.HttpConnectionTimeout);
            Assert.AreEqual(15000, result.HttpReadTimeout);
            Assert.AreEqual(false, result.RandomizeRefreshRates);
            Assert.AreEqual(150, result.TreatmentLogRefreshRate);
            Assert.AreEqual(30000, result.TreatmentLogSize);
            Assert.AreEqual(60, result.EventLogRefreshRate);
            Assert.AreEqual(5000, result.EventLogSize);
            Assert.AreEqual(10, result.EventsFirstPushWindow);
            Assert.AreEqual(1000, result.MaxCountCalls);
            Assert.AreEqual(60, result.MaxTimeBetweenCalls);
            Assert.AreEqual(5, result.NumberOfParalellSegmentTasks);
            Assert.AreEqual(false, result.StreamingEnabled);
            Assert.AreEqual(1, result.AuthRetryBackoffBase);
            Assert.AreEqual(1, result.StreamingReconnectBackoffBase);
            Assert.AreEqual("https://auth.split.io/api/auth", result.AuthServiceURL);
            Assert.AreEqual("https://streaming.split.io/event-stream", result.StreamingServiceURL);
            Assert.AreEqual(ImpressionModes.Debug, result.ImpressionMode);
            Assert.AreEqual("ip-test", result.SdkMachineIP);
            Assert.AreEqual("name-test", result.SdkMachineName);
            Assert.AreEqual("version-test", result.SdkSpecVersion);
            Assert.AreEqual("version-test", result.SdkVersion);
            Assert.AreEqual(true, result.LabelsEnabled);
        }

        [TestMethod]
        public void GetRedisDefatulConfig()
        {
            // Arrange.
            _wrapperAdapter
                .Setup(mock => mock.ReadConfig(It.IsAny<ConfigurationOptions>(), It.IsAny<ISplitLogger>()))
                .Returns(new ReadConfigData
                {
                    SdkMachineIP = "ip-test",
                    SdkMachineName = "name-test",
                    SdkSpecVersion = "version-test",
                    SdkVersion = "version-test",
                });

            // Act.
            var result = _configService.ReadConfig(new ConfigurationOptions(), ConfingTypes.Redis);

            // Assert
            Assert.AreEqual("ip-test", result.SdkMachineIP);
            Assert.AreEqual("name-test", result.SdkMachineName);
            Assert.AreEqual("version-test", result.SdkSpecVersion);
            Assert.AreEqual("version-test", result.SdkVersion);
            Assert.AreEqual(true, result.LabelsEnabled);
        }
    }
}
