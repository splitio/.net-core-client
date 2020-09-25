using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Impressions.Classes;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class ImpressionsObserverTests
    {
        [TestMethod]
        public void TestAndSet()
        {
            var impressionsObserver = new ImpressionsObserver(new ImpressionHasher());

            var impression = new KeyImpression
            {
                keyName = "matching_key",
                bucketingKey = "bucketing_key",
                feature = "split_name",
                treatment = "treatment",
                label = "default label",
                changeNumber = 1533177602748,
                time = 1478113516022
            };

            var impression2 = new KeyImpression
            {
                keyName = "matching_key_2",
                bucketingKey = "bucketing_key_2",
                feature = "split_name_2",
                treatment = "treatment_2",
                label = "default label_2",
                changeNumber = 1533177602748,
                time = 1478113516022
            };

            var result = impressionsObserver.TestAndSet(impression);
            Assert.IsNull(result);

            // Should return previous time
            impression.time = 1478113516500;
            result = impressionsObserver.TestAndSet(impression);
            Assert.AreEqual(1478113516022, result);

            // Should return the new impression.time
            result = impressionsObserver.TestAndSet(impression);
            Assert.AreEqual(1478113516500, result);

            // When impression.time < previous should return the min.
            impression.time = 1478113516001;
            result = impressionsObserver.TestAndSet(impression);
            Assert.AreEqual(1478113516001, result);

            // Should return null because is another impression
            result = impressionsObserver.TestAndSet(impression2);
            Assert.IsNull(result);

            // Should return previous time
            impression2.time = 1478113516500;
            result = impressionsObserver.TestAndSet(impression2);
            Assert.AreEqual(1478113516022, result);

            // Should return null because the impression is null
            result = impressionsObserver.TestAndSet(null);
            Assert.IsNull(result);
        }
    }
}
