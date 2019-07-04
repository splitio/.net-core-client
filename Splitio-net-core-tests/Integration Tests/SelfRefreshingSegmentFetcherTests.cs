using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.CommonLibraries;
using System.Threading;
using Splitio.Domain;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.Client.Classes;
using Splitio.Services.Cache.Classes;
using System.Collections.Concurrent;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class SelfRefreshingSegmentFetcherTests
    {
        private readonly string rootFilePath;

        public SelfRefreshingSegmentFetcherTests()
        {
#if NETCORE
            rootFilePath = @"Resources\";
#endif
        }

        [TestMethod]
        [DeploymentItem(@"Resources\segment_payed.json")]
        public void ExecuteGetSuccessfulWithResultsFromJSONFile()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());

            var segmentFetcher = new JSONFileSegmentFetcher($"{rootFilePath}segment_payed.json", segmentCache);

            //Act
            segmentFetcher.InitializeSegment("payed");

            //Assert
            Assert.IsTrue(segmentCache.IsInSegment("payed", "abcdz"));
        }


        [TestMethod]
        [Ignore] 
        public void ExecuteGetSuccessfulWithResults()
        {
            //Arrange
            var baseUrl = "https://sdk-aws-staging.split.io/api/";
            var httpHeader = new HTTPHeader()
            {
                authorizationApiKey = "///PUT API KEY HERE///",
                splitSDKMachineIP = "1.0.0.0",
                splitSDKMachineName = "localhost",
                splitSDKVersion = "net-0.0.0",
                splitSDKSpecVersion = "1.2",
            };
            var sdkApiClient = new SegmentSdkApiClient(httpHeader, baseUrl, 10000, 10000);
            var apiSegmentChangeFetcher = new ApiSegmentChangeFetcher(sdkApiClient);
            var gates = new InMemoryReadinessGatesCache();
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var selfRefreshingSegmentFetcher = new SelfRefreshingSegmentFetcher(apiSegmentChangeFetcher, gates, 30, segmentCache, 4);

            //Act
            var name = "payed";
            selfRefreshingSegmentFetcher.InitializeSegment(name);

            while(!gates.AreSegmentsReady(1000))
            {
                Thread.Sleep(10);
            }

            //Assert
            Assert.IsTrue(segmentCache.IsInSegment(name, "abcdz"));
        }

    }
}
