using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.CommonLibraries;
using Splitio.Services.SplitFetcher.Classes;


namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class SplitSdkApiClientTests
    {
        [TestMethod]
        [Ignore]
        public async void ExecuteFetchSplitChangesSuccessful()
        {
            //Arrange
            var baseUrl = "http://sdk-aws-staging.split.io/api/";
            var httpHeader = new HTTPHeader()
            {
                authorizationApiKey = "///PUT API KEY HERE///",
                splitSDKMachineIP = "1.0.0.0",
                splitSDKMachineName = "localhost",
                splitSDKVersion = "net-0.0.0",
                splitSDKSpecVersion = "1.2",
            };
            var SplitSdkApiClient = new SplitSdkApiClient(httpHeader, baseUrl, 10000, 10000);

            //Act
            var result = await SplitSdkApiClient.FetchSplitChanges(-1);
  
            //Assert
            Assert.IsTrue(result.Contains("splits"));
            
        }


        [TestMethod]
        public async void ExecuteGetShouldReturnEmptyIfNotAuthorized()
        {
            //Arrange
            var baseUrl = "https://sdk.aws.staging.split.io/api";
            var httpHeader = new HTTPHeader()
            {
                splitSDKMachineIP = "1.0.0.0",
                splitSDKMachineName = "localhost",
                splitSDKVersion = "net-0.0.0",
                splitSDKSpecVersion = "1.2"
            };
            var SplitSdkApiClient = new SplitSdkApiClient(httpHeader, baseUrl, 10000, 10000);

            //Act
            var result = await SplitSdkApiClient.FetchSplitChanges(-1);

            //Assert
            Assert.IsTrue(result == String.Empty);
        }
    }
}
