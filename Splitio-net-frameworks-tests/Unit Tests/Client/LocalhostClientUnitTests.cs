using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Shared.Interfaces;

namespace Splitio_net_frameworks_tests.Unit_Tests.Client
{
    [TestClass]
    public class LocalhostClientUnitTests
    {
        private readonly Mock<ILog> _logMock;
        private readonly Mock<IFactoryInstantiationsService> _factoryInstantiationsService;

        public LocalhostClientUnitTests()
        {
            _logMock = new Mock<ILog>();
            _factoryInstantiationsService = new Mock<IFactoryInstantiationsService>();
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldReturnControlIfSplitNotFound()
        {
            //Arrange
            var splitClient = new LocalhostClient("test.splits", _logMock.Object, _factoryInstantiationsService.Object);
            
            //Act
            var result = splitClient.GetTreatment("test", "test");

            //Assert
            Assert.AreEqual("control", result);
        }
        
        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void GetTreatmentShouldRunAsSingleKeyUsingNullBucketingKey()
        {
            var splitClient = new LocalhostClient("test.splits", _logMock.Object, _factoryInstantiationsService.Object);

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
            var splitClient = new LocalhostClientForTesting(@"test.splits", _logMock.Object, _factoryInstantiationsService.Object);

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
            var splitClient = new LocalhostClientForTesting("test.splits", _logMock.Object, _factoryInstantiationsService.Object);

            //Act
            splitClient.Destroy();

            //Assert
            Assert.IsTrue(splitClient.IsDestroyed());
            _factoryInstantiationsService.Verify(mock => mock.Decrease("localhost"), Times.Once());
        }

        [TestMethod]
        [DeploymentItem(@"Resources\test.splits")]
        public void Destroy_WhenIsDestroyed()
        {
            //Arrange
            var splitClient = new LocalhostClientForTesting("test.splits", _logMock.Object, _factoryInstantiationsService.Object, isDestroyed: true);

            //Act
            splitClient.Destroy();

            //Assert
            Assert.IsTrue(splitClient.IsDestroyed());
            _factoryInstantiationsService.Verify(mock => mock.Decrease(It.IsAny<string>()), Times.Never());
        }
    }
}
