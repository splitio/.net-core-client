using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Client.Classes;
using Splitio.Services.Parsing.Classes;
using Splitio.Services.SegmentFetcher.Classes;
using Splitio.Services.SplitFetcher.Classes;
using System.Collections.Concurrent;

namespace Splitio_net_frameworks_tests.Integration_Tests
{
    [TestClass]
    public class SelfRefreshingSplitFetcherTests
    {
        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging.json")]
        [DeploymentItem(@"Resources\segment_payed.json")]

        public void ExecuteGetSuccessfulWithResultsFromJSONFile()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var splitParser = new InMemorySplitParser(new JSONFileSegmentFetcher("segment_payed.json", segmentCache), segmentCache);
            var splitChangeFetcher = new JSONFileSplitChangeFetcher("splits_staging.json");
            var splitChangesResult = splitChangeFetcher.Fetch(-1);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());         
            var gates = new InMemoryReadinessGatesCache();
            var selfRefreshingSplitFetcher = new SelfRefreshingSplitFetcher(splitChangeFetcher, splitParser, gates, 30, splitCache);
            selfRefreshingSplitFetcher.Start();
            gates.IsSDKReady(1000);

            //Act           
            ParsedSplit result = (ParsedSplit)splitCache.GetSplit("Pato_Test_1");

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Pato_Test_1");
            Assert.IsTrue(result.conditions.Count > 0);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_4.json")]
        [DeploymentItem(@"Resources\segment_payed.json")]
        public void ExecuteGetSuccessfulWithResultsFromJSONFileIncludingTrafficAllocation()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var splitParser = new InMemorySplitParser(new JSONFileSegmentFetcher("segment_payed.json", segmentCache), segmentCache);
            var splitChangeFetcher = new JSONFileSplitChangeFetcher("splits_staging_4.json");
            var splitChangesResult = splitChangeFetcher.Fetch(-1);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var gates = new InMemoryReadinessGatesCache();
            var selfRefreshingSplitFetcher = new SelfRefreshingSplitFetcher(splitChangeFetcher, splitParser, gates, 30, splitCache);
            selfRefreshingSplitFetcher.Start();
            gates.IsSDKReady(1000);

            //Act           
            ParsedSplit result = (ParsedSplit)splitCache.GetSplit("Traffic_Allocation_UI");

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Traffic_Allocation_UI");
            Assert.IsTrue(result.trafficAllocation == 100);
            Assert.IsTrue(result.trafficAllocationSeed == 0);
            Assert.IsTrue(result.conditions.Count > 0);
            Assert.IsNotNull(result.conditions.Find(x => x.conditionType == ConditionType.ROLLOUT));
        }

        [TestMethod]
        [Ignore] 
        public void ExecuteGetSuccessfulWithResults()
        {
            //Arrange
            var baseUrl = "https://sdk-aws-staging.split.io/api/";
            //var baseUrl = "http://localhost:3000/api/";
            var httpHeader = new HTTPHeader()
            {
                authorizationApiKey = "///PUT API KEY HERE///",
                splitSDKMachineIP = "1.0.0.0",
                splitSDKMachineName = "localhost",
                splitSDKVersion = "net-0.0.0",
                splitSDKSpecVersion = "1.2",
            };
            var sdkApiClient = new SplitSdkApiClient(httpHeader, baseUrl, 10000, 10000);
            var apiSplitChangeFetcher = new ApiSplitChangeFetcher(sdkApiClient);
            var sdkSegmentApiClient = new SegmentSdkApiClient(httpHeader, baseUrl, 10000, 10000);
            var apiSegmentChangeFetcher = new ApiSegmentChangeFetcher(sdkSegmentApiClient);
            var gates = new InMemoryReadinessGatesCache();
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var selfRefreshingSegmentFetcher = new SelfRefreshingSegmentFetcher(apiSegmentChangeFetcher, gates, 30, segmentCache, 4);

            var splitParser = new InMemorySplitParser(selfRefreshingSegmentFetcher, segmentCache);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var selfRefreshingSplitFetcher = new SelfRefreshingSplitFetcher(apiSplitChangeFetcher, splitParser, gates, 30, splitCache);
            selfRefreshingSplitFetcher.Start();

            //Act           
            gates.IsSDKReady(1000);
            selfRefreshingSplitFetcher.Stop();
            ParsedSplit result  = (ParsedSplit)splitCache.GetSplit("Pato_Test_1");
            ParsedSplit result2 = (ParsedSplit)splitCache.GetSplit("Manu_Test_1");
            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.name == "Pato_Test_1");
            Assert.IsTrue(result.conditions.Count > 0);

        }

        [TestMethod]
        public void ExecuteGetWithoutResults()
        {
            //Arrange
            var baseUrl = "https://sdk-aws-staging.split.io/api/";
            var httpHeader = new HTTPHeader()
            {
                authorizationApiKey = "0",
                splitSDKMachineIP = "1.0.0.0",
                splitSDKMachineName = "localhost",
                splitSDKVersion = "net-0.0.0",
                splitSDKSpecVersion = "1.2",
            };
            var sdkApiClient = new SplitSdkApiClient(httpHeader, baseUrl, 10000, 10000);
            var apiSplitChangeFetcher = new ApiSplitChangeFetcher(sdkApiClient);
            var sdkSegmentApiClient = new SegmentSdkApiClient(httpHeader, baseUrl, 10000, 10000);
            var apiSegmentChangeFetcher = new ApiSegmentChangeFetcher(sdkSegmentApiClient);
            var gates = new InMemoryReadinessGatesCache();
          
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());

            var selfRefreshingSegmentFetcher = new SelfRefreshingSegmentFetcher(apiSegmentChangeFetcher, gates, 30, segmentCache, 4);
            var splitParser = new InMemorySplitParser(selfRefreshingSegmentFetcher, segmentCache);
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var selfRefreshingSplitFetcher = new SelfRefreshingSplitFetcher(apiSplitChangeFetcher, splitParser, gates, 30, splitCache);
            selfRefreshingSplitFetcher.Start();

            //Act
            gates.IsSDKReady(10);

            var result = splitCache.GetSplit("condition_and");

            //Assert
            Assert.IsNull(result);      
        }
    }
}
