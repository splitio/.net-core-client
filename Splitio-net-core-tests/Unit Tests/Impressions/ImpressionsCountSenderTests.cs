using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.Impressions.Interfaces;
using Splitio_Tests.Resources;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class ImpressionsCountSenderTests
    {
        private readonly Mock<ITreatmentSdkApiClient> _apiClient;

        public ImpressionsCountSenderTests()
        {
            _apiClient = new Mock<ITreatmentSdkApiClient>();
            
        }

        [TestMethod]
        public void Start_ShouldSendImpressionsCount()
        {
            // Arrange.
            var impressionsCounter = new ImpressionsCounter();
            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 15, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 50, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature2", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 50, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature3", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 50, 11, DateTimeKind.Utc)));

            var impressionsCountSender = new ImpressionsCountSender(_apiClient.Object, impressionsCounter, 1);

            // Act.
            impressionsCountSender.Start();

            // Assert.
            Thread.Sleep(1500);
            _apiClient.Verify(mock => mock.SendBulkImpressionsCount(It.IsAny<ConcurrentDictionary<KeyCache, int>>()), Times.Once);
        }

        [TestMethod]
        public void Start_ShouldNotSendImpressionsCount()
        {
            // Arrange.
            var impressionsCounter = new ImpressionsCounter();
            var impressionsCountSender = new ImpressionsCountSender(_apiClient.Object, impressionsCounter, 1);

            // Act.
            impressionsCountSender.Start();

            // Assert.
            Thread.Sleep(1500);
            _apiClient.Verify(mock => mock.SendBulkImpressionsCount(It.IsAny<ConcurrentDictionary<KeyCache, int>>()), Times.Never);
        }

        [TestMethod]
        public void Stop_ShouldSendImpressionsCount()
        {
            // Arrange.
            var impressionsCounter = new ImpressionsCounter();
            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 15, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 50, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature2", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 50, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature3", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 50, 11, DateTimeKind.Utc)));

            var impressionsCountSender = new ImpressionsCountSender(_apiClient.Object, impressionsCounter);

            // Act.
            impressionsCountSender.Start();
            Thread.Sleep(1000);
            impressionsCountSender.Stop();

            // Assert.
            _apiClient.Verify(mock => mock.SendBulkImpressionsCount(It.IsAny<ConcurrentDictionary<KeyCache, int>>()), Times.Once);
        }
    }
}
