using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.CommonLibraries;
using Splitio.Services.SplitFetcher.Classes;


namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class SplitSdkApiClientTests
    {
        [TestInitialize]
        public void Initialization()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        [TestMethod]
        [Ignore]
        public void ExecuteFetchSplitChangesSuccessful()
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
                encoding = "gzip"
            };
            var SplitSdkApiClient = new SplitSdkApiClient(httpHeader, baseUrl, 10000, 10000);

            //Act
            var result = SplitSdkApiClient.FetchSplitChanges(-1);
  
            //Assert
            Assert.IsTrue(result.Contains("splits"));
            
        }


        [TestMethod]
        public void ExecuteGetShouldReturnEmptyIfNotAuthorized()
        {
            //Arrange
            var baseUrl = "https://sdk.aws.staging.split.io/api";
            var httpHeader = new HTTPHeader()
            {
                encoding = "gzip",
                splitSDKMachineIP = "1.0.0.0",
                splitSDKMachineName = "localhost",
                splitSDKVersion = "net-0.0.0",
                splitSDKSpecVersion = "1.2"
            };
            var SplitSdkApiClient = new SplitSdkApiClient(httpHeader, baseUrl, 10000, 10000);

            //Act
            var result = SplitSdkApiClient.FetchSplitChanges(-1);

            //Assert
            Assert.IsTrue(result == String.Empty);
        }
    }
}
