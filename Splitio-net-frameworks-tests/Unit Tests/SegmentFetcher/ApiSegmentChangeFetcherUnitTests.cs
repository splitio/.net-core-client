using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Threading.Tasks;

namespace Splitio_net_frameworks_tests.Unit_Tests.SegmentFetcher
{
    [TestClass]
    public class ApiSegmentChangeFetcherUnitTests
    {
        [TestMethod]
        public async void FetchSegmentChangesSuccessfull()
        {
            //Arrange
            var apiClient = new Mock<ISegmentSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSegmentChanges(It.IsAny<string>(), It.IsAny<long>()))
            .Returns(TaskEx.FromResult(@"{
                          'name': 'payed',
                          'added': [
                            'abcdz',
                            'bcadz',
                            'xzydz'
                          ],
                          'removed': [],
                          'since': -1,
                          'till': 1470947453877
                        }"));
            var apiFetcher = new ApiSegmentChangeFetcher(apiClient.Object);
            
            //Act
            var result = await apiFetcher.Fetch("payed", -1);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("payed", result.name);
            Assert.AreEqual(-1, result.since);
            Assert.AreEqual(1470947453877, result.till);
            Assert.AreEqual(3, result.added.Count);
            Assert.AreEqual(0, result.removed.Count);
        }

        [TestMethod]
        public async void FetchSegmentChangesWithExcepionSouldReturnNull()
        {
            var apiClient = new Mock<ISegmentSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSegmentChanges(It.IsAny<string>(), It.IsAny<long>()))
            .Throws(new Exception());
            var apiFetcher = new ApiSegmentChangeFetcher(apiClient.Object);
           
            //Act
            var result = await apiFetcher.Fetch("payed", -1);

            //Assert
            Assert.IsNull(result);
        }
    }
}
