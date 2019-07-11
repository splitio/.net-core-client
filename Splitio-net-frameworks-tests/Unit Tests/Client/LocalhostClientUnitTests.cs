using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Client.Classes;

namespace Splitio_net_frameworks_tests.Unit_Tests.Client
{
    [TestClass]
    public class LocalhostClientUnitTests
    {
        private readonly Mock<ILog> _logMock;

        public LocalhostClientUnitTests()
        {
            _logMock = new Mock<ILog>();
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldReturnControlIfSplitNotFound()
        {
            //Arrange
            var splitClient = new LocalhostClient("test.splits", _logMock.Object);
            
            //Act
            var result = splitClient.GetTreatment("test", "test");

            //Assert
            Assert.AreEqual("control", result);
        }
        
        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldRunAsSingleKeyUsingNullBucketingKey()
        {
            var splitClient = new LocalhostClient("test.splits", _logMock.Object);

            //Act
            var key = new Key("test", null);
            var result = splitClient.GetTreatment(key, "other_test_feature");

            //Assert
            Assert.AreEqual(key.bucketingKey, key.matchingKey);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void TrackShouldNotStoreEvents()
        {
            //Arrange
            var splitClient = new LocalhostClientForTesting(@"test.splits", _logMock.Object);

            //Act
            var result = splitClient.Track("test", "test", "test");

            //Assert
            Assert.AreEqual(true, result);
            Assert.IsNull(splitClient.GetEventListener());
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void Destroy()
        {
            //Arrange
            var splitClient = new LocalhostClientForTesting("test.splits", _logMock.Object);

            //Act
            splitClient.Destroy();

            //Assert
            Assert.IsTrue(splitClient.IsDestroyed());
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void Destroy_WhenIsDestroyed()
        {
            //Arrange
            var splitClient = new LocalhostClientForTesting("test.splits", _logMock.Object, isDestroyed: true);

            //Act
            splitClient.Destroy();

            //Assert
            Assert.IsTrue(splitClient.IsDestroyed());
        }
    }
}
