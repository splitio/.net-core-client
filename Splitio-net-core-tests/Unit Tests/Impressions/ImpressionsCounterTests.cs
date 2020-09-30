using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Impressions.Classes;
using Splitio_Tests.Resources;
using System;
using System.Linq;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class ImpressionsCounterTests
    {
        [TestMethod]
        public void IncBasicUsage()
        {
            // Arrange.
            var impressionsCounter = new ImpressionsCounter();

            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 15, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 20, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 50, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature2", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 50, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature2", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 55, 11, DateTimeKind.Utc)));
            impressionsCounter.Inc("feature1", SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 50, 11, DateTimeKind.Utc)));

            // Act.
            var result = impressionsCounter.PopAll();

            // Assert.
            Assert.AreEqual(3, result.FirstOrDefault(x => x.Key.Equals($"feature1::{SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 0, 0, DateTimeKind.Utc))}")).Value);
            Assert.AreEqual(2, result.FirstOrDefault(x => x.Key.Equals($"feature2::{SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 0, 0, DateTimeKind.Utc))}")).Value);
            Assert.AreEqual(1, result.FirstOrDefault(x => x.Key.Equals($"feature1::{SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 0, 0, DateTimeKind.Utc))}")).Value);
        }
    }
}
