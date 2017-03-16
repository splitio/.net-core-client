using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;
using Splitio.Domain;
using log4net;
using log4net.Repository.Hierarchy;

namespace Splitio_Tests.Unit_Tests.Client
{
    [TestClass]
    public class LocalhostClientUnitTests
    {
        [TestInitialize]
        public void Initialize()
        {
            try
            {
                var respository = LogManager.GetRepository("splitio");
            }
            catch
            {
                LogManager.CreateRepository("splitio", typeof(Hierarchy));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldReturnControlIfSplitNotFound()
        {
            //Arrange
            var splitClient = new LocalhostClient("test.splits");
            
            //Act
            var result = splitClient.GetTreatment("test", "test");

            //Assert
            Assert.AreEqual("control", result);
        }
        
        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldRunAsSingleKeyUsingNullBucketingKey()
        {
            var splitClient = new LocalhostClient("test.splits");

            //Act
            var key = new Key("test", null);
            var result = splitClient.GetTreatment(key, "other_test_feature");

            //Assert
            Assert.AreEqual(key.bucketingKey, key.matchingKey);
        }
    }
}
