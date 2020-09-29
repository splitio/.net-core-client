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

            var config = (SelfRefreshingConfig)_configService.ReadConfig(new ConfigurationOptions(), ConfingTypes.InMemory);

            Assert.AreEqual(true, config.LabelsEnabled);
            Assert.AreEqual("https://sdk.split.io", config.BaseUrl);
            Assert.AreEqual("https://events.split.io", config.EventsBaseUrl);
            Assert.AreEqual(5, config.SplitsRefreshRate);
            Assert.AreEqual(60, config.SegmentRefreshRate);
            Assert.AreEqual(15000, config.HttpConnectionTimeout);
            Assert.AreEqual(15000, config.HttpReadTimeout);
            Assert.AreEqual(false, config.RandomizeRefreshRates);
            Assert.AreEqual(30, config.TreatmentLogRefreshRate);
            Assert.AreEqual(30000, config.TreatmentLogSize);
            Assert.AreEqual(60, config.EventLogRefreshRate);
            Assert.AreEqual(5000, config.EventLogSize);
            Assert.AreEqual(10, config.EventsFirstPushWindow);
            Assert.AreEqual(1000, config.MaxCountCalls);
            Assert.AreEqual(60, config.MaxTimeBetweenCalls);
            Assert.AreEqual(5, config.NumberOfParalellSegmentTasks);
            Assert.AreEqual(true, config.StreamingEnabled);
            Assert.AreEqual(1, config.AuthRetryBackoffBase);
            Assert.AreEqual(1, config.StreamingReconnectBackoffBase);
            Assert.AreEqual("https://auth.split.io/api/auth", config.AuthServiceURL);
            Assert.AreEqual("https://streaming.split.io/event-stream", config.StreamingServiceURL);
            Assert.AreEqual(ImpressionModes.Optimized, config.ImpressionMode);
            Assert.AreEqual("ip-test", config.SdkMachineIP);
            Assert.AreEqual("name-test", config.SdkMachineName);
            Assert.AreEqual("version-test", config.SdkSpecVersion);
            Assert.AreEqual("version-test", config.SdkVersion);
            Assert.AreEqual(true, config.LabelsEnabled);
        }

        [TestMethod]
        public void GetRedisDefatulConfig()
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

            var config = _configService.ReadConfig(new ConfigurationOptions(), ConfingTypes.Redis);

            Assert.AreEqual("ip-test", config.SdkMachineIP);
            Assert.AreEqual("name-test", config.SdkMachineName);
            Assert.AreEqual("version-test", config.SdkSpecVersion);
            Assert.AreEqual("version-test", config.SdkVersion);
            Assert.AreEqual(true, config.LabelsEnabled);
        }
    }
}
