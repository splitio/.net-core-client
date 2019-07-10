using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Shared.Classes;

namespace Splitio_Tests.Unit_Tests.Client
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
            var splitClient = new LocalhostClient(@"Resources\test.splits", _logMock.Object);
            splitClient.BlockUntilReady(1000);

            //Act
            var result = splitClient.GetTreatment("test", "test");

            //Assert
            Assert.AreEqual("control", result);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldRunAsSingleKeyUsingNullBucketingKey()
        {
            //Arrange
            var splitClient = new LocalhostClient(@"Resources\test.splits", _logMock.Object);
            splitClient.BlockUntilReady(1000);

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
            var splitClient = new LocalhostClientForTesting(@"Resources\test.splits", _logMock.Object);
            splitClient.BlockUntilReady(1000);

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
            var _factoryInstantiationsService = FactoryInstantiationsService.Instance(_logMock.Object);
            var splitClient = new LocalhostClientForTesting(@"Resources\test.splits", _logMock.Object);
            
            //Act
            splitClient.BlockUntilReady(1000);
            splitClient.Destroy();
            var result = ((FactoryInstantiationsService)_factoryInstantiationsService).GetInstantiations();

            //Assert
            Assert.IsTrue(splitClient.IsDestroyed());
            Assert.IsFalse(result.IsEmpty);
            
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void Destroy_WhenIsDestroyed()
        {
            //Arrange
            var _factoryInstantiationsService = FactoryInstantiationsService.Instance(_logMock.Object);
            var splitClient = new LocalhostClientForTesting(@"Resources\test.splits", _logMock.Object, isDestroyed: true);

            //Act
            splitClient.BlockUntilReady(1000);
            splitClient.Destroy();
            var result = ((FactoryInstantiationsService)_factoryInstantiationsService).GetInstantiations();

            //Assert
            Assert.IsTrue(splitClient.IsDestroyed());
            Assert.IsFalse(result.IsEmpty);
        }
    }
}
