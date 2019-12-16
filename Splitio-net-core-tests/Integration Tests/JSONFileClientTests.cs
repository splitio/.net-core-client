using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Client.Classes;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.InputValidation.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio_Tests.Integration_Tests
{
//    [TestClass]
//    public class JSONFileClientTests
//    {
//        private readonly Mock<ISplitLogger> _logMock;
//        private readonly string rootFilePath;

//        public JSONFileClientTests()
//        {
//            _logMock = new Mock<ISplitLogger>();

//            // This line is to clean the warnings.
//            rootFilePath = string.Empty;

//#if NETCORE
//            rootFilePath = @"Resources\";
//#endif
//        }

//        #region GetTreatment
//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentOnFailedParsingSplitShouldReturnControl()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object);

//            //Act           
//            var result = client.GetTreatment("test", "fail", null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("control", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentOnFailedParsingSplitShouldNotAffectOtherSplits()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test", "asd", null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentOnDeletedSplitShouldReturnControl()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test", "asd", null);
//            client.RemoveSplitFromCache("asd");
//            var result2 = client.GetTreatment("test", "asd", null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result);
//            Assert.IsNotNull(result2);
//            Assert.AreEqual("control", result2);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentOnExceptionShouldReturnControl()
//        {
//            //Arrange
//            //var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var splitCacheMock = new Mock<ISplitCache>();

//            splitCacheMock
//                .Setup(x => x.GetSplit(It.IsAny<string>()))
//                .Throws<Exception>();

//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, splitCacheMock.Object);

//            //Act           
//            var result = client.GetTreatment("test", "asd", null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("control", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [DeploymentItem(@"Resources\segment_payed.json")]
//        public void ExecuteGetTreatmentOnRemovedUserFromSegmentShouldReturnOff()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", $"{rootFilePath}segment_payed.json", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("abcdz", "test_jw2", null);
//            client.RemoveKeyFromSegmentCache("payed", new List<string>() { "abcdz" });
//            var result2 = client.GetTreatment("abcdz", "test_jw2", null);


//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result);
//            Assert.IsNotNull(result2);
//            Assert.AreEqual("off", result2);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_4.json")]
//        public void ExecuteGetTreatmentOnSplitWithOnOffOnPartition()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_4.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("01", "Test_on_off_on", null);
//            var result2 = client.GetTreatment("a0ax09z", "Test_on_off_on", null);
//            var result3 = client.GetTreatment("00b0", "Test_on_off_on", null);


//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result);
//            Assert.IsNotNull(result2);
//            Assert.AreEqual("off", result2);
//            Assert.IsNotNull(result3);
//            Assert.AreEqual("on", result3);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_4.json")]
//        public void ExecuteGetTreatmentOnSplitWithTrafficAllocation()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_4.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("01", "Traffic_Allocation_UI", null);
//            var result2 = client.GetTreatment("ab", "Traffic_Allocation_UI", null);
//            var result3 = client.GetTreatment("00b0", "Traffic_Allocation_UI", null);


//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result);
//            Assert.IsNotNull(result2);
//            Assert.AreEqual("off", result2);
//            Assert.IsNotNull(result3);
//            Assert.AreEqual("off", result3);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_4.json")]
//        public void ExecuteGetTreatmentOnSplitWithTrafficAllocationWhenAllocationIsDifferentThan100()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_4.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("01", "Traffic_Allocation_UI3", null);
//            var result2 = client.GetTreatment("ab", "Traffic_Allocation_UI3", null);
//            var result3 = client.GetTreatment("00b0", "Traffic_Allocation_UI3", null);


//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result);
//            Assert.IsNotNull(result2);
//            Assert.AreEqual("off", result2);
//            Assert.IsNotNull(result3);
//            Assert.AreEqual("off", result3);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_7.json")]
//        public void ExecuteGetTreatmentOnSplitWithTrafficAllocationWhenAllocationIs1ReturnsRolloutTreatment()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_7.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("aaaaaaklmnbv", "ta_bucket1_test", null);

//            //Assert
//            Assert.AreEqual("rollout_treatment", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_7.json")]
//        public void ExecuteGetTreatmentOnSplitWithTrafficAllocationWhenAllocationIs1ReturnsDefaultTreatment()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_7.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("mauro_test", "ta_bucket1_test", null);

//            //Assert
//            Assert.AreEqual("default_treatment", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentOnSplitWithSegmentNotInitialized()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            //feature test_jw2 has UserDefinedSegmentMatcher 
//            //on "payed" segment, and it is not initialized.
//            var result = client.GetTreatment("abcdz", "test_jw2", null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentAndLogLabelKilled()
//        {
//            //Arrange
//            //var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test", "test_jw3", null);

//            //Assert
//            //treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "test" && 
//            //                                                                       p.FirstOrDefault().feature == "test_jw3" && 
//            //                                                                       p.FirstOrDefault().treatment == "off" && 
//            //                                                                       p.FirstOrDefault().time > 0 && 
//            //                                                                       p.FirstOrDefault().changeNumber == 1470947806420 && 
//            //                                                                       p.FirstOrDefault().label == "killed" && 
//            //                                                                       p.FirstOrDefault().bucketingKey == null)));
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentAndLogLabelNoConditionMatched()
//        {
//            //Arrange
//            //var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test", "whitelisting_elements", null);

//            //Assert
//            //treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "test" && 
//            //                                                                       p.FirstOrDefault().feature == "whitelisting_elements" && 
//            //                                                                       p.FirstOrDefault().treatment == "off" && 
//            //                                                                       p.FirstOrDefault().time > 0 && 
//            //                                                                       p.FirstOrDefault().changeNumber == 1471368078203 && 
//            //                                                                       p.FirstOrDefault().label == "default rule" && 
//            //                                                                       p.FirstOrDefault().bucketingKey == null)));

//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentAndLogLabelSplitNotFound()
//        {
//            //Arrange
//            //var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null);

//            //Act           
//            client.RemoveSplitFromCache("asd");
//            var result = client.GetTreatment("test", "asd", null);

//            //Assert
//            Assert.AreEqual("control", result);
//            //treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "test" &&
//            //                                                                       p.FirstOrDefault().feature == "asd" &&
//            //                                                                       p.FirstOrDefault().treatment == "control" &&
//            //                                                                       p.FirstOrDefault().time > 0 &&
//            //                                                                       p.FirstOrDefault().changeNumber == null &&
//            //                                                                       p.FirstOrDefault().label == "definition not found" &&
//            //                                                                       p.FirstOrDefault().bucketingKey == null)), Times.Never);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentAndLogLabelException()
//        {
//            //Arrange
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var splitCacheMock = new Mock<ISplitCache>();

//            splitCacheMock
//                .Setup(x => x.GetSplit(It.IsAny<string>()))
//                .Throws<Exception>();

//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, splitCacheMock.Object, treatmentLogMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test", "asd", null);

//            //Assert
//            treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "test" &&
//                                                                                   p.FirstOrDefault().feature == "asd" &&
//                                                                                   p.FirstOrDefault().treatment == "control" &&
//                                                                                   p.FirstOrDefault().time > 0 &&
//                                                                                   p.FirstOrDefault().changeNumber == null &&
//                                                                                   p.FirstOrDefault().label == "exception" &&
//                                                                                   p.FirstOrDefault().bucketingKey == null)));
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_4.json")]
//        public void ExecuteGetTreatmentAndLogLabelTrafficAllocationFailed()
//        {
//            //Arrange
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_4.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test", "Traffic_Allocation_UI2", null);

//            //Assert
//            treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "test" &&
//                                                                                   p.FirstOrDefault().feature == "Traffic_Allocation_UI2" &&
//                                                                                   p.FirstOrDefault().treatment == "off" &&
//                                                                                   p.FirstOrDefault().time > 0 &&
//                                                                                   p.FirstOrDefault().changeNumber == 1490652849498 &&
//                                                                                   p.FirstOrDefault().label == "not in split" &&
//                                                                                   p.FirstOrDefault().bucketingKey == null)));
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentAndLogLabelForTreatment()
//        {
//            //Arrange
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("db765170-e9f2-11e5-885c-c2f58c3a47a7", "Segments_Restructuring_UI", null);

//            //Assert
//            treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "db765170-e9f2-11e5-885c-c2f58c3a47a7" &&
//                                                                                   p.FirstOrDefault().feature == "Segments_Restructuring_UI" &&
//                                                                                   p.FirstOrDefault().treatment == "on" &&
//                                                                                   p.FirstOrDefault().time > 0 &&
//                                                                                   p.FirstOrDefault().changeNumber == 1484084207827 &&
//                                                                                   p.FirstOrDefault().label == "explicitly included" &&
//                                                                                   p.FirstOrDefault().bucketingKey == null)));
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentWhenUnknownMatcherIsIncluded()
//        {
//            //Arrange
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            //Act           
//            var result = client.GetTreatment("xs", "Unknown_Matcher", null);

//            //Assert
//            Assert.AreEqual("control", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentAndNotLogLabelForTreatmentIfLabelsNotEnabled()
//        {
//            //Arrange
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object, isLabelsEnabled: false);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("db765170-e9f2-11e5-885c-c2f58c3a47a7", "Segments_Restructuring_UI", null);

//            //Assert
//            treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "db765170-e9f2-11e5-885c-c2f58c3a47a7" &&
//                                                                                   p.FirstOrDefault().feature == "Segments_Restructuring_UI" &&
//                                                                                   p.FirstOrDefault().treatment == "on" &&
//                                                                                   p.FirstOrDefault().time > 0 &&
//                                                                                   p.FirstOrDefault().changeNumber == 1484084207827 &&
//                                                                                   p.FirstOrDefault().label == null &&
//                                                                                   p.FirstOrDefault().bucketingKey == null)));
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentAndLogLabelAndBucketingKeyForTreatment()
//        {
//            //Arrange
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var key = new Key("db765170-e9f2-11e5-885c-c2f58c3a47a7", "ab765170-e9f2-11e5-885c-c2f58c3a47a7");
//            var result = client.GetTreatment(key, "Segments_Restructuring_UI", null);

//            //Assert
//            treatmentLogMock.Verify(x => x.Notify(It.Is<IList<KeyImpression>>(p => p.FirstOrDefault().keyName == "db765170-e9f2-11e5-885c-c2f58c3a47a7" &&
//                                                                                   p.FirstOrDefault().feature == "Segments_Restructuring_UI" &&
//                                                                                   p.FirstOrDefault().treatment == "on" &&
//                                                                                   p.FirstOrDefault().time > 0 &&
//                                                                                   p.FirstOrDefault().changeNumber == 1484084207827 &&
//                                                                                   p.FirstOrDefault().label == "explicitly included" &&
//                                                                                   p.FirstOrDefault().bucketingKey == "ab765170-e9f2-11e5-885c-c2f58c3a47a7")));

//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_4.json")]
//        public void ExecuteGetTreatmentWithBooleanAttribute()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_4.json", "", _logMock.Object, null, null);

//            var attributes = new Dictionary<string, object>
//            {
//                { "boolean_attribute", true }
//            };

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("fake_id_1", "sample_feature_bug", attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_5.json")]
//        public void ExecuteGetTreatmentWithSetMatcherReturnsOff()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_5.json", "", _logMock.Object);

//            var attributes = new Dictionary<string, object>
//            {
//                { "permissions", new List<string>() { "create" } }
//            };

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test1", "UT_NOT_SET_MATCHER", attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result); // !Contains any of "create","delete","update"
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_5.json")]
//        public void ExecuteGetTreatmentWithSetMatcherReturnsOn()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_5.json", "", _logMock.Object);

//            var attributes = new Dictionary<string, object>
//            {
//                { "permissions", new List<string>() { "execute" } }
//            };

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test1", "UT_NOT_SET_MATCHER", attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result); // !Contains any of "create","delete","update"
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_5.json")]
//        public void ExecuteGetTreatmentWithStringMatcherReturnsOff()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_5.json", "", _logMock.Object);

//            var attributes = new Dictionary<string, object>
//            {
//                { "st", "permission" }
//            };

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test1", "string_matchers", attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result); // Starts with "a" or "b" --> 100% off
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_5.json")]
//        public void ExecuteGetTreatmentWithStringMatcherReturnsOn()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_5.json", "", _logMock.Object);

//            var attributes = new Dictionary<string, object>
//            {
//                { "st", "allow" }
//            };

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test1", "string_matchers", attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result); // Starts with "a" or "b" --> 100% off
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_6.json")]
//        public void ExecuteGetTreatmentWithDependencyMatcherReturnsOn()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_6.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("fake_user_id_1", "test_dependency", null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_6.json")]
//        public void ExecuteGetTreatmentWithDependencyMatcherReturnsOff()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_6.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("fake_user_id_6", "test_dependency", null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("off", result);
//        }

//        //[TestMethod]
//        //[DeploymentItem(@"Resources\splits_staging_6.json")]
//        //public void ExecuteGetTreatmentWithDependencyMatcherImpressionOnChild()
//        //{
//        //    //Arrange
//        //    var impressionsCache = new InMemorySimpleCache<KeyImpression>(new BlockingQueue<KeyImpression>(10));
//        //    var selfUpdatingTreatmentLog = new SelfUpdatingTreatmentLog(null, 1000, impressionsCache);
//        //    //var treatmentLogInstance = new AsynchronousListener<IList<KeyImpression>>(WrapperAdapter.GetLogger("AsynchronousImpressionListener"));
//        //    //treatmentLogInstance.AddListener(selfUpdatingTreatmentLog);

//        //    var client = new JSONFileClient($"{rootFilePath}splits_staging_6.json", "", _logMock.Object, null, null, treatmentLogInstance);

//        //    client.BlockUntilReady(1000);

//        //    //Act           
//        //    var result = client.GetTreatment("test", "test_dependency_segment", null);

//        //    //Assert
//        //    Assert.IsNotNull(result);
//        //    Assert.AreEqual("V1", result);

//        //    Thread.Sleep(2000);
//        //    var items = impressionsCache.FetchAllAndClear();
//        //    Assert.AreEqual(1, items.Count);
//        //    Assert.AreEqual(items.FirstOrDefault().feature, "test_dependency_segment");
//        //}

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatment_WhenNameDoesntExist_DontLogImpression()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);
//            var splitName = "not_exist";

//            client.BlockUntilReady(1000);

//            // Act.
//            var result = client.GetTreatment("key", splitName);

//            // Assert.
//            Assert.AreEqual("control", result);
//            _logMock.Verify(mock => mock.Warn($"GetTreatment: you passed {splitName} that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
//            treatmentLogMock.Verify(x => x.Notify(It.IsAny<IList<KeyImpression>>()), Times.Never);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatment_WithoutBlockUntiltReady_ReturnsOff()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            // Act.
//            var result = client.GetTreatment("key", "anding");

//            // Assert.
//            Assert.AreEqual("off", result);
//            _logMock.Verify(mock => mock.Error($"GetTreatment: the SDK is not ready, the operation cannot be executed."), Times.Never);
//        }
//        #endregion

//        #region GetTreatments
//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatments()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object);
//            List<string> features = new List<string>
//            {
//                "fail",
//                "asd",
//                "get_environment"
//            };

//            var attributes = new Dictionary<string, object>
//            {
//                { "env", "test" }
//            };

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatments("test", features, attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("control", result["fail"]);
//            Assert.AreEqual("off", result["asd"]);
//            Assert.AreEqual("test", result["get_environment"]);

//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        public void ExecuteGetTreatmentsWithBucketing()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object);
//            List<string> features = new List<string>
//            {
//                "fail",
//                "asd",
//                "get_environment"
//            };

//            var attributes = new Dictionary<string, object>
//            {
//                { "env", "test" }
//            };

//            var keys = new Key("test", "test");

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatments(keys, features, attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("control", result["fail"]);
//            Assert.AreEqual("off", result["asd"]);
//            Assert.AreEqual("test", result["get_environment"]);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_6.json")]
//        public void ExecuteGetTreatmentsWithDependencyMatcherReturnsOn()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_6.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var features = new List<string>
//            {
//                "test_whitelist",
//                "test_dependency"
//            };
//            var result = client.GetTreatments("fake_user_id_1", features, null);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result["test_whitelist"]);
//            Assert.AreEqual("on", result["test_dependency"]);
//        }

//        [TestMethod]
//        [DeploymentItem(@"Resources\splits_staging_6.json")]
//        public void ExecuteGetTreatmentsWithDependencyMatcherWithAttributesReturnsOn()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_6.json", "", _logMock.Object);

//            client.BlockUntilReady(1000);

//            //Act           
//            var features = new List<string>
//            {
//                "test_whitelist",
//                "test_dependency"
//            };
//            var attributes = new Dictionary<string, object>
//            {
//                { "st", "allow" }
//            };
//            var result = client.GetTreatments("fake_user_id_1", features, attributes);

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result["test_whitelist"]);
//            Assert.AreEqual("on", result["test_dependency"]);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatments_WhenNameDoesntExist_DontLogImpression()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);
//            var splitNames = new List<string> { "not_exist", "not_exist_1" };

//            client.BlockUntilReady(1000);

//            // Act.
//            var result = client.GetTreatments("key", splitNames);

//            // Assert.
//            foreach (var res in result)
//            {
//                Assert.AreEqual("control", res.Value);
//            }

//            foreach (var name in splitNames)
//            {
//                _logMock.Verify(mock => mock.Warn($"GetTreatment: you passed {name} that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
//            }

//            treatmentLogMock.Verify(x => x.Notify(It.IsAny<IList<KeyImpression>>()), Times.Never);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatments_WithoutBlockUntiltReady_ReturnsEmptyList()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            // Act.
//            var result = client.GetTreatments("key", new List<string>());

//            // Assert.
//            Assert.IsTrue(result.Count == 0);
//            _logMock.Verify(mock => mock.Error($"GetTreatment: the SDK is not ready, the operation cannot be executed."), Times.Never);
//        }
        
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatments_WithoutBlockUntiltReady_ReturnsTreatments()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            // Act.
//            var result = client.GetTreatments("key", new List<string> { "anding", "in_ten_keys" });

//            // Assert.
//            var treatment1 = result.FirstOrDefault(r => r.Key.Equals("anding"));
//            Assert.AreEqual("off", treatment1.Value);
            
//            var treatment2 = result.FirstOrDefault(r => r.Key.Equals("in_ten_keys"));
//            Assert.AreEqual("on", treatment2.Value);

//            _logMock.Verify(mock => mock.Error($"GetTreatment: the SDK is not ready, the operation cannot be executed."), Times.Never);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatments_WhenClientIsReadyAndFeaturesIsEmpty_ReturnsEmptyList()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);
//            client.BlockUntilReady(100);

//            // Act.
//            var result = client.GetTreatments("key", new List<string>());

//            // Assert.
//            Assert.IsTrue(result.Count == 0);
//        }
//        #endregion

//        #region Destroy
//        [DeploymentItem(@"Resources\splits_staging_5.json")]
//        [TestMethod]
//        public void DestroySucessfully()
//        {
//            //Arrange
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_5.json", "", _logMock.Object);

//            var attributes = new Dictionary<string, object>
//            {
//                { "permissions", new List<string>() { "execute" } }
//            };

//            client.BlockUntilReady(1000);

//            //Act           
//            var result = client.GetTreatment("test1", "UT_NOT_SET_MATCHER", attributes);
//            client.Destroy();
//            var resultDestroy1 = client.GetTreatment("test1", "UT_NOT_SET_MATCHER", attributes);
//            var manager = client.GetSplitManager();
//            var resultDestroy2 = manager.Splits();
//            var resultDestroy3 = manager.SplitNames();
//            var resultDestroy4 = manager.Split("UT_NOT_SET_MATCHER");

//            //Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("on", result); // !Contains any of "create","delete","update"
//            Assert.IsTrue(resultDestroy1 == "control");
//            Assert.AreEqual(resultDestroy2.Count, 0);
//            Assert.AreEqual(resultDestroy3.Count, 0);
//            Assert.IsTrue(resultDestroy4 == null);
//        }
//        #endregion

//        #region Track
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void Track_WhenClientIsNotReady_ReturnsTrue()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var eventListenerMock = new Mock<IAsynchronousListener<WrappedEvent>>();            
//            var trafficTypeValidator = new Mock<ITrafficTypeValidator>();

//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object, eventListener: eventListenerMock.Object, trafficTypeValidator: trafficTypeValidator.Object);

//            trafficTypeValidator
//                .Setup(mock => mock.IsValid(It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(new ValidatorResult { Success = true }); ;

//            // Act.
//            var result = client.Track("key", "traffic_type", "event_type");

//            // Assert.
//            Assert.IsTrue(result);
//        }
//        #endregion

//        #region GetTreatmentWithConfig
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatmentWithConfig_WhenNameDoesntExist_DontLogImpression()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);
//            var splitName = "not_exist";

//            client.BlockUntilReady(1000);

//            // Act.
//            var result = client.GetTreatmentWithConfig("key", splitName);

//            // Assert.
//            Assert.AreEqual("control", result.Treatment);
//            Assert.IsNull(result.Config);
//            _logMock.Verify(mock => mock.Warn($"GetTreatment: you passed {splitName} that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
//            treatmentLogMock.Verify(x => x.Notify(It.IsAny<IList<KeyImpression>>()), Times.Never);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatmentWithConfig_WithoutBlockUntiltReady_ReturnsOff()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            // Act.
//            var result = client.GetTreatmentWithConfig("key", "anding");

//            // Assert.
//            Assert.AreEqual("off", result.Treatment);
//            Assert.IsNull(result.Config);
//            _logMock.Verify(mock => mock.Error($"GetTreatment: the SDK is not ready, the operation cannot be executed."), Times.Never);
//        }
//        #endregion

//        #region GetTreatmentsWithConfig
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatmentsWithConfig_WhenNameDoesntExist_DontLogImpression()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);
//            var splitNames = new List<string> { "not_exist", "not_exist_1" };

//            client.BlockUntilReady(1000);

//            // Act.
//            var result = client.GetTreatmentsWithConfig("key", splitNames);

//            // Assert.
//            foreach (var res in result)
//            {
//                Assert.AreEqual("control", res.Value.Treatment);
//                Assert.IsNull(res.Value.Config);

//                _logMock.Verify(mock => mock.Warn($"GetTreatment: you passed {res.Key} that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
//            }

//            treatmentLogMock.Verify(x => x.Notify(It.IsAny<IList<KeyImpression>>()), Times.Never);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatmentsWithConfig_WithoutBlockUntiltReady_ReturnsEmptyList()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            // Act.
//            var result = client.GetTreatmentsWithConfig("anding", new List<string>());

//            // Assert.
//            Assert.IsTrue(result.Count == 0);
//            _logMock.Verify(mock => mock.Error($"GetTreatment: the SDK is not ready, the operation cannot be executed."), Times.Never);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatmentsWithConfig_WithoutBlockUntiltReady_ReturnsTreatments()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);

//            // Act.
//            var result = client.GetTreatmentsWithConfig("key", new List<string> { "anding", "whitelisting_elements" });

//            // Assert.
//            var treatment1 = result.FirstOrDefault(r => r.Key.Equals("anding"));
//            Assert.AreEqual("off", treatment1.Value.Treatment);
//            Assert.IsNull(treatment1.Value.Config);

//            var treatment2 = result.FirstOrDefault(r => r.Key.Equals("whitelisting_elements"));
//            Assert.AreEqual("off", treatment2.Value.Treatment);
//            Assert.IsNull(treatment2.Value.Config);

//            _logMock.Verify(mock => mock.Error($"GetTreatment: the SDK is not ready, the operation cannot be executed."), Times.Never);
//        }

//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void GetTreatmentsWithConfig_WhenClientIsReadyAndFeaturesIsEmpty_ReturnsEmptyList()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);
//            client.BlockUntilReady(100);

//            // Act.
//            var result = client.GetTreatmentsWithConfig("key", new List<string>());

//            // Assert.
//            Assert.IsTrue(result.Count == 0);
//        }
//        #endregion

//        #region Manager-Split
//        [DeploymentItem(@"Resources\splits_staging_3.json")]
//        [TestMethod]
//        public void Split_Manager_WhenNameDoesntExist_ReturnsNull()
//        {
//            // Arrange.
//            var treatmentLogMock = new Mock<IAsynchronousListener<IList<KeyImpression>>>();
//            var client = new JSONFileClient($"{rootFilePath}splits_staging_3.json", "", _logMock.Object, null, null, treatmentLogMock.Object);
//            var manager = client.GetSplitManager();
//            var splitName = "not_exist";

//            manager.BlockUntilReady(1000);

//            // Act.
//            var result = manager.Split(splitName);

//            // Assert.
//            Assert.IsNull(result);
//            _logMock.Verify(mock => mock.Warn($"split: you passed {splitName} that does not exist in this environment, please double check what Splits exist in the web console."), Times.Once);
//        }
//        #endregion
//    }
}