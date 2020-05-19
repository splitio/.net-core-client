using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.Client.Classes;
using System.Collections.Concurrent;
using Splitio.Services.Cache.Classes;
using Splitio.Domain;
using Moq;
using Splitio.Services.SplitFetcher.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Splitio.Services.Cache.Interfaces;

namespace Splitio_Tests.Unit_Tests.SegmentFetcher
{
    [TestClass]
    public class SelfRefreshingSegmentFetcherUnitTests
    {
        [TestMethod]
        public void InitializeSegmentNotExistent()
        {
            //Arrange
            var gates = new InMemoryReadinessGatesCache();
            var apiClient = new Mock<ISegmentSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSegmentChanges(It.IsAny<string>(), It.IsAny<long>()))
            .Returns(Task.FromResult(@"{
                          'name': 'payed',
                          'added': [
                            'abcdz',
                            'bcadz',
                            'xzydz'
                          ],
                          'removed': [],
                          'since': -1,
                          'till': 10001
                        }"));
            var apiFetcher = new ApiSegmentChangeFetcher(apiClient.Object);
            var segments = new ConcurrentDictionary<string, Segment>();
            var cache = new InMemorySegmentCache(segments);
            var segmentFetcher = new SelfRefreshingSegmentFetcher(apiFetcher, gates, 10, cache, 1);
            
            //Act
            segmentFetcher.InitializeSegment("payed");

            //Assert
            Thread.Sleep(5000);
            Assert.IsTrue(gates.AreSegmentsReady(1));
            Assert.IsTrue(cache.IsInSegment("payed", "abcdz"));
        }

        [TestMethod]
        public void StartSchedullerSuccessfully()
        {
            //Arrange
            var gates = new Mock<IReadinessGatesCache>();
            var apiClient = new Mock<ISegmentSdkApiClient>();
            apiClient
                .Setup(x => x.FetchSegmentChanges(It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Task.FromResult(@"{
                              'name': 'payed',
                              'added': [
                                'abcdz',
                                'bcadz',
                                'xzydz'
                              ],
                              'removed': [],
                              'since': -1,
                              'till': 10001
                            }"));

            gates
                .Setup(mock => mock.IsSDKReady(0))
                .Returns(true);

            var apiFetcher = new ApiSegmentChangeFetcher(apiClient.Object);
            var segments = new ConcurrentDictionary<string, Segment>();
            var cache = new InMemorySegmentCache(segments);
            var segmentFetcher = new SelfRefreshingSegmentFetcher(apiFetcher, gates.Object, 10, cache, 1);

            //Act
            segmentFetcher.InitializeSegment("payed");
            segmentFetcher.Start();

            //Assert
            Assert.IsTrue(SegmentTaskQueue.segmentsQueue.TryTake(out SelfRefreshingSegment segment, -1));
        }
    }
}
