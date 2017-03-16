using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Splitio.Services.EngineEvaluator;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class SplitterTests
    {
        [DeploymentItem(@"Resources\sample-data.csv")]
        [TestMethod]
        public void VerifyHashAndBucketSampleData()
        {
            //Arrange
            var contents = File.ReadAllText("sample-data.csv").Split('\n');
            var csv = from line in contents
                      select line.Split(',').ToArray();

            var splitter = new Splitter();
            bool first = true;

            //Act
            foreach (string[] item in csv)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    var hash = splitter.Hash(item[1], int.Parse(item[0]));
                    var bucket = splitter.Bucket(item[1], int.Parse(item[0]));
                    //Assert
                    Assert.AreEqual(hash, int.Parse(item[2]));
                    Assert.AreEqual(bucket, int.Parse(item[3]));
                }
            }
        }

        [DeploymentItem(@"Resources\sample-data-non-alpha-numeric.csv")]
        [TestMethod]
        public void VerifyHashAndBucketSampleDataNonAlphanumeric()
        {
            //Arrange
            var contents = File.ReadAllText("sample-data-non-alpha-numeric.csv", System.Text.Encoding.BigEndianUnicode).Split('\n');
            var csv = from line in contents
                      select line.Split(',').ToArray();

            var splitter = new Splitter();
            bool first = true;

            //Act
            foreach (string[] item in csv)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    var hash = splitter.Hash(item[1], int.Parse(item[0]));
                    var bucket = splitter.Bucket(item[1], int.Parse(item[0]));
                    //Assert
                    Assert.AreEqual(hash, int.Parse(item[2]));
                    Assert.AreEqual(bucket, int.Parse(item[3]));
                }
            }
        }
    }
}
