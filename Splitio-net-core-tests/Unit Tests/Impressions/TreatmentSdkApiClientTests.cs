using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Impressions.Classes;
using Splitio_Tests.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

            var treatmentSdkApiClient = new TreatmentSdkApiClient(new HTTPHeader(), "http://www.fake-test-split.com", 5, 5);
            
            // Act.
            var result = treatmentSdkApiClient.ConvertToJson(impressions);

            // Assert.
            var expected = $"{{\"pf\":[{{\"f\":\"featur1\",\"m\":{time9am},\"rc\":2}}]}}";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CorrectFormatSendImpressions()
        {
            // Arrange.
            var impressions = new List<KeyImpression>
            {
                new KeyImpression("matching-key", "feature-1", "treatment", 34534546, 3333444, "label", "bucketing-key"),
                new KeyImpression("matching-key", "feature-1", "treatment", 34534550, 3333444, "label", "bucketing-key", 34534546),
                new KeyImpression("matching-key", "feature-2", "treatment", 34534546, 3333444, "label", "bucketing-key"),
            };

            var treatmentSdkApiClient = new TreatmentSdkApiClient(new HTTPHeader(), "http://www.fake-test-split.com", 5, 5);

            // Act.
            var result = treatmentSdkApiClient.ConvertToJson(impressions);

            // Assert.
            var expected = "[{\"f\":\"feature-1\",\"i\":[{\"k\":\"matching-key\",\"t\":\"treatment\",\"m\":34534546,\"c\":3333444,\"r\":\"label\",\"b\":\"bucketing-key\"},{\"k\":\"matching-key\",\"t\":\"treatment\",\"m\":34534550,\"c\":3333444,\"r\":\"label\",\"b\":\"bucketing-key\"}]},{\"f\":\"feature-2\",\"i\":[{\"k\":\"matching-key\",\"t\":\"treatment\",\"m\":34534546,\"c\":3333444,\"r\":\"label\",\"b\":\"bucketing-key\"}]}]";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CorrectFormatSendImpressions()
        {
            // Arrange.
            var impressions = new List<KeyImpression>
            {
                new KeyImpression("matching-key", "feature-1", "treatment", 34534546, 3333444, "label", "bucketing-key"),
                new KeyImpression("matching-key", "feature-1", "treatment", 34534550, 3333444, "label", "bucketing-key", 34534546),
                new KeyImpression("matching-key", "feature-2", "treatment", 34534546, 3333444, "label", "bucketing-key"),
            };

            var treatmentSdkApiClient = new TreatmentSdkApiClient(new HTTPHeader(), "http://www.fake-test-split.com", 5, 5);

            // Act.
            var result = treatmentSdkApiClient.ConvertToJson(impressions);

            // Assert.
            var expected = "[{\"f\":\"feature-1\",\"i\":[{\"k\":\"matching-key\",\"t\":\"treatment\",\"m\":34534546,\"c\":3333444,\"r\":\"label\",\"b\":\"bucketing-key\"},{\"k\":\"matching-key\",\"t\":\"treatment\",\"m\":34534550,\"c\":3333444,\"r\":\"label\",\"b\":\"bucketing-key\"}]},{\"f\":\"feature-2\",\"i\":[{\"k\":\"matching-key\",\"t\":\"treatment\",\"m\":34534546,\"c\":3333444,\"r\":\"label\",\"b\":\"bucketing-key\"}]}]";
            Assert.AreEqual(expected, result);
        }
    }
}
