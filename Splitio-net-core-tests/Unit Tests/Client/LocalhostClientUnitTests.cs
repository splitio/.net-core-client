using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;
using Splitio.Domain;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class LocalhostClientUnitTests
    {
        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldReturnControlIfSplitNotFound()
        {
            //Arrange
            var splitClient = new LocalhostClient(@"Resources\test.splits");
            
            //Act
            var result = splitClient.GetTreatment("test", "test");

            //Assert
            Assert.AreEqual("control", result);
        }
        
        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldRunAsSingleKeyUsingNullBucketingKey()
        {
            var splitClient = new LocalhostClient(@"Resources\test.splits");

            //Act
            var key = new Key("test", null);
            var result = splitClient.GetTreatment(key, "other_test_feature");

            //Assert
            Assert.AreEqual(key.bucketingKey, key.matchingKey);
        }
    }
}
