using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Impressions.Classes;
using System;
using System.IO;
using System.Linq;

namespace Splitio_Tests.Unit_Tests.Impressions
{
    [TestClass]
    public class ImpressionHasherTests
    {
        private readonly string rootFilePath;

        public ImpressionHasherTests()
        {
            // This line is to clean the warnings.
            rootFilePath = string.Empty;

#if NETCORE
            rootFilePath = @"Resources\";
#endif
        }

        [TestMethod]
        public void Works()
        {
            var impressionHasher = new ImpressionHasher();

            var impression = new KeyImpression
            {
                feature = "someFeature",
                keyName = "someKeyName",
                treatment = "someTreatment",
                changeNumber = 3245463,
                label = "someLabel"
            };

            var impression2 = new KeyImpression
            {
                feature = "someFeature",
                keyName = "someKeyName",
                treatment = "otherTreatment",
                changeNumber = 3245463,
                label = "someLabel"
            };

            var result = impressionHasher.Process(impression);
            var result2 = impressionHasher.Process(impression2);
            Assert.AreNotEqual(result, result2);

            impression2.keyName = "otherKeyName";
            var result3 = impressionHasher.Process(impression2);
            Assert.AreNotEqual(result2, result3);

            impression2.feature = "otherFeature";
            var result4 = impressionHasher.Process(impression2);
            Assert.AreNotEqual(result3, result4);

            impression2.treatment = "treatment";
            var result5 = impressionHasher.Process(impression2);
            Assert.AreNotEqual(result4, result5);

            impression2.label = "otherLabel";
            var result6 = impressionHasher.Process(impression2);
            Assert.AreNotEqual(result5, result6);

            impression2.changeNumber = 888755;
            var result7 = impressionHasher.Process(impression2);
            Assert.AreNotEqual(result6, result7);
        }

        [TestMethod]
        public void DoesNotCrash()
        {
            var impressionHasher = new ImpressionHasher();

            var impression = new KeyImpression
            {
                feature = null,
                keyName = "someKeyName",
                treatment = "someTreatment",
                changeNumber = 3245463,
                label = "someLabel"
            };

            Assert.IsNotNull(impressionHasher.Process(impression));

            impression.keyName = null;
            Assert.IsNotNull(impressionHasher.Process(impression));

            impression.changeNumber = null;
            Assert.IsNotNull(impressionHasher.Process(impression));

            impression.label = null;
            Assert.IsNotNull(impressionHasher.Process(impression));

            impression.treatment = null;
            Assert.IsNotNull(impressionHasher.Process(impression));
        }

        [DeploymentItem(@"Resources\murmur3-64-128.csv")]
        [TestMethod]
        public void TestingMurmur128WithCsv()
        {
            var impressionHasher = new ImpressionHasher();

            var fileContent = File.ReadAllText($"{rootFilePath}murmur3-64-128.csv");
            var contents = fileContent.Split(new string[] { "\n" }, StringSplitOptions.None);
            var csv = contents.Select(x => x.Split(',')).ToArray();

            foreach (string[] item in csv)
            {
                if (item.Length != 3)
                    continue;

                var key = item[0];
                var seed = uint.Parse(item[1]);
                var expected = ulong.Parse(item[2]);

                var result = impressionHasher.Hash(key, seed);

                Assert.AreEqual(expected, result);
            }
        }
    }
}
