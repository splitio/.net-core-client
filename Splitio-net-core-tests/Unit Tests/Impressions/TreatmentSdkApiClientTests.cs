using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.CommonLibraries;
using Splitio.Services.Impressions.Classes;
using Splitio_Tests.Resources;
using System;
using System.Collections.Concurrent;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class TreatmentSdkApiClientTests
    {
        [TestMethod]
        public void CorrectFormatSendCounts()
        {
            // Arrange.
            var time9am = SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 09, 00, 00, DateTimeKind.Utc));
            var time10am = SplitsHelper.MakeTimestamp(new DateTime(2020, 09, 02, 10, 00, 00, DateTimeKind.Utc));

            var impressions = new ConcurrentDictionary<KeyCache, int>();
            impressions.TryAdd(new KeyCache("featur1", time9am), 2);
            impressions.TryAdd(new KeyCache("featur2", time9am), 1);
            impressions.TryAdd(new KeyCache("featur3", time9am), 1);
            impressions.TryAdd(new KeyCache("featur2", time10am), 2);            

            var treatmentSdkApiClient = new TreatmentSdkApiClient(new HTTPHeader(), "http://www.fake-test-split.com", 5, 5);
            
            // Act.
            var result = treatmentSdkApiClient.ConvertToJson(impressions, true);

            // Assert.
            var expected = $"{{\"pf\":[{{\"t\":\"featur1\",\"m\":{time9am},\"rc\":2}},{{\"t\":\"featur2\",\"m\":{time9am},\"rc\":1}},{{\"t\":\"featur2\",\"m\":{time10am},\"rc\":2}},{{\"t\":\"featur3\",\"m\":{time9am},\"rc\":1}}]}}";
            Assert.AreEqual(expected, result);
        }
    }
}
