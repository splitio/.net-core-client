using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.SplitFetcher.Interfaces;
using Moq;
using Splitio.Services.Client.Classes;
using Splitio.Services.Cache.Classes;
using System.Collections.Concurrent;
using Splitio.Domain;
using System.Threading.Tasks;

namespace Splitio_Tests.Unit_Tests.SegmentFetcher
{
    [TestClass]
    public class SelfRefreshingSegmentUnitTests
    {
        [TestMethod]
        public void RefreshSegmentNullChangesFetcherResponseShouldNotUpdateCache()
        {
            //Arrange
            var gates = new InMemoryReadinessGatesCache();
            var apiClient = new Mock<ISegmentSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSegmentChanges(It.IsAny<string>(), It.IsAny<long>()))
            .Throws(new Exception());
            var apiFetcher = new ApiSegmentChangeFetcher(apiClient.Object);
            var segments  = new ConcurrentDictionary<string, Segment>();
            var cache = new InMemorySegmentCache(segments);
            var segmentFetcher = new SelfRefreshingSegment("payed", apiFetcher, gates, cache);
            
            //Act
            segmentFetcher.RefreshSegment();

            //Assert
            Assert.AreEqual(0, segments.Count);
        }

        [TestMethod]
        public void RefreshSegmentShouldUpdateReadinessGatesWhenNoMoreChanges()
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
                          'till': -1
                        }"));
            var apiFetcher = new ApiSegmentChangeFetcher(apiClient.Object);
            var segments = new ConcurrentDictionary<string, Segment>();
            var cache = new InMemorySegmentCache(segments);
            var segmentFetcher = new SelfRefreshingSegment("payed", apiFetcher, gates, cache);

            //Act
            segmentFetcher.RefreshSegment();

            //Assert
            Assert.IsTrue(gates.AreSegmentsReady(1));
        }

        [TestMethod]
        public void RefreshSegmentShouldUpdateSegmentsCache()
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
            var segmentFetcher = new SelfRefreshingSegment("payed", apiFetcher, gates, cache);

            //Act
            segmentFetcher.RefreshSegment();

            //Assert
            Assert.AreEqual(1, segments.Count);
        }
    }
}
