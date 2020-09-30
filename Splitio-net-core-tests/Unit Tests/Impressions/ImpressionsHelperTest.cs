using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Impressions.Classes;
using Splitio_Tests.Resources;
using System;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class ImpressionsHelperTest
    {
        [TestMethod]
        public void TruncateTimeFrame()
        {
            var expected = SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(expected, ImpressionsHelper.TruncateTimeFrame(SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 53, 12, DateTimeKind.Utc))));
            Assert.AreEqual(expected, ImpressionsHelper.TruncateTimeFrame(SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 00, 00, DateTimeKind.Utc))));
            Assert.AreEqual(expected, ImpressionsHelper.TruncateTimeFrame(SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 53, 00, DateTimeKind.Utc))));
            Assert.AreEqual(expected, ImpressionsHelper.TruncateTimeFrame(SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 00, 12, DateTimeKind.Utc))));

            expected = SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 00, 00, 00, DateTimeKind.Utc));
            Assert.AreEqual(expected, ImpressionsHelper.TruncateTimeFrame(SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 00, 00, 00, DateTimeKind.Utc))));
        }        
    }
}
