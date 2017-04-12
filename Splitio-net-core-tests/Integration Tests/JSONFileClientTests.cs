using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Client.Classes;
using System.Collections.Generic;
using Moq;
using Splitio.Services.Impressions.Interfaces;
using Splitio.Services.Cache.Interfaces;
using Splitio.Domain;

namespace Splitio_Tests.Integration_Tests
{
    [TestClass]
    public class JSONFileClientTests
    {
        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentOnFailedParsingSplitShouldReturnControl()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "");

            //Act           
            var result = client.GetTreatment("test", "fail", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentOnFailedParsingSplitShouldNotAffectOtherSplits()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "");

            //Act           
            var result = client.GetTreatment("test", "asd", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentOnDeletedSplitShouldReturnControl()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "");

            //Act           
            var result = client.GetTreatment("test", "asd", null);
            client.RemoveSplitFromCache("asd");
            var result2 = client.GetTreatment("test", "asd", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
            Assert.IsNotNull(result2);
            Assert.AreEqual("control", result2);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentOnExceptionShouldReturnControl()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var splitCacheMock = new Mock<ISplitCache>();
            splitCacheMock.Setup(x => x.GetSplit(It.IsAny<string>())).Throws<Exception>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, splitCacheMock.Object, treatmentLogMock.Object);

            //Act           
            var result = client.GetTreatment("test", "asd", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatments()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "");
            List<string> features = new List<string>();
            features.Add("fail");
            features.Add("asd");
            features.Add("get_environment");

            var attributes = new Dictionary<string, object>();
            attributes.Add("env", "test");

            //Act           
            var result = client.GetTreatments("test", features, attributes);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result["fail"]);
            Assert.AreEqual("off", result["asd"]);
            Assert.AreEqual("test", result["get_environment"]);

        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentsWithBucketing()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "");
            List<string> features = new List<string>();
            features.Add("fail");
            features.Add("asd");
            features.Add("get_environment");

            var attributes = new Dictionary<string, object>();
            attributes.Add("env", "test");

            var keys = new Key("test", "test");

            //Act           
            var result = client.GetTreatments(keys, features, attributes);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("control", result["fail"]);
            Assert.AreEqual("off", result["asd"]);
            Assert.AreEqual("test", result["get_environment"]);
        }


        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        [DeploymentItem(@"Resources\segment_payed.json")]
        public void ExecuteGetTreatmentOnRemovedUserFromSegmentShouldReturnOff()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", @"Resources\segment_payed.json");

            //Act           
            var result = client.GetTreatment("abcdz", "test_jw2", null);
            client.RemoveKeyFromSegmentCache("payed", new List<string>() { "abcdz" });
            var result2 = client.GetTreatment("abcdz", "test_jw2", null);


            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
            Assert.IsNotNull(result2);
            Assert.AreEqual("off", result2);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_4.json")]
        public void ExecuteGetTreatmentOnSplitWithOnOffOnPartition()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_4.json", "");

            //Act           
            var result = client.GetTreatment("01", "Test_on_off_on", null);
            var result2 = client.GetTreatment("ab", "Test_on_off_on", null);
            var result3 = client.GetTreatment("00b0", "Test_on_off_on", null);


            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("on", result);
            Assert.IsNotNull(result2);
            Assert.AreEqual("off", result2);
            Assert.IsNotNull(result3);
            Assert.AreEqual("on", result3);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_4.json")]
        public void ExecuteGetTreatmentOnSplitWithTrafficAllocation()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_4.json", "");

            //Act           
            var result = client.GetTreatment("01", "Traffic_Allocation_UI", null);
            var result2 = client.GetTreatment("ab", "Traffic_Allocation_UI", null);
            var result3 = client.GetTreatment("00b0", "Traffic_Allocation_UI", null);


            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
            Assert.IsNotNull(result2);
            Assert.AreEqual("off", result2);
            Assert.IsNotNull(result3);
            Assert.AreEqual("off", result3);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_4.json")]
        public void ExecuteGetTreatmentOnSplitWithTrafficAllocationWhenAllocationIsDifferentThan100()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_4.json", "");

            //Act           
            var result = client.GetTreatment("01", "Traffic_Allocation_UI3", null);
            var result2 = client.GetTreatment("ab", "Traffic_Allocation_UI3", null);
            var result3 = client.GetTreatment("00b0", "Traffic_Allocation_UI3", null);


            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
            Assert.IsNotNull(result2);
            Assert.AreEqual("off", result2);
            Assert.IsNotNull(result3);
            Assert.AreEqual("off", result3);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentOnSplitWithSegmentNotInitialized()
        {
            //Arrange
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "");

            //Act           
            //feature test_jw2 has UserDefinedSegmentMatcher 
            //on "payed" segment, and it is not initialized.
            var result = client.GetTreatment("abcdz", "test_jw2", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("off", result);
        }


        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentAndLogLabelKilled()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, null, treatmentLogMock.Object);

            //Act           
            var result = client.GetTreatment("test", "test_jw3", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "test" && p.feature == "test_jw3" && p.treatment == "off" && p.time > 0 && p.changeNumber == 1470947806420 && p.label == "killed" && p.bucketingKey == null)));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentAndLogLabelNoConditionMatched()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, null, treatmentLogMock.Object);

            //Act           
            var result = client.GetTreatment("test", "whitelisting_elements", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "test" && p.feature == "whitelisting_elements" && p.treatment == "off" && p.time > 0 && p.changeNumber == 1471368078203 && p.label == "no rule matched" && p.bucketingKey == null)));

        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentAndLogLabelSplitNotFound()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, null, treatmentLogMock.Object);

            //Act           
            client.RemoveSplitFromCache("asd");
            var result = client.GetTreatment("test", "asd", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "test" && p.feature == "asd" && p.treatment == "control" && p.time > 0 && p.changeNumber == null && p.label == "rules not found" && p.bucketingKey == null)));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentAndLogLabelException()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var splitCacheMock = new Mock<ISplitCache>();
            splitCacheMock.Setup(x => x.GetSplit(It.IsAny<string>())).Throws<Exception>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, splitCacheMock.Object, treatmentLogMock.Object);

            //Act           
            var result = client.GetTreatment("test", "asd", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "test" && p.feature == "asd" && p.treatment == "control" && p.time > 0 && p.changeNumber == null && p.label == "exception" && p.bucketingKey == null)));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_4.json")]
        public void ExecuteGetTreatmentAndLogLabelTrafficAllocationFailed()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var client = new JSONFileClient(@"Resources\splits_staging_4.json", "", null, null, treatmentLogMock.Object);

            //Act           
            var result = client.GetTreatment("test", "Traffic_Allocation_UI2", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "test" && p.feature == "Traffic_Allocation_UI2" && p.treatment == "off" && p.time > 0 && p.changeNumber == 1490652849498 && p.label == "not in split" && p.bucketingKey == null)));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentAndLogLabelForTreatment()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, null, treatmentLogMock.Object);

            //Act           
            var result = client.GetTreatment("db765170-e9f2-11e5-885c-c2f58c3a47a7", "Segments_Restructuring_UI", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "db765170-e9f2-11e5-885c-c2f58c3a47a7" && p.feature == "Segments_Restructuring_UI" && p.treatment == "on" && p.time > 0 && p.changeNumber == 1484084207827 && p.label == "explicitly included" && p.bucketingKey == null)));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentAndNotLogLabelForTreatmentIfLabelsNotEnabled()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, null, treatmentLogMock.Object, isLabelsEnabled: false);

            //Act           
            var result = client.GetTreatment("db765170-e9f2-11e5-885c-c2f58c3a47a7", "Segments_Restructuring_UI", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "db765170-e9f2-11e5-885c-c2f58c3a47a7" && p.feature == "Segments_Restructuring_UI" && p.treatment == "on" && p.time > 0 && p.changeNumber == 1484084207827 && p.label == null && p.bucketingKey == null)));
        }

        [TestMethod]
        [DeploymentItem(@"Resources\splits_staging_3.json")]
        public void ExecuteGetTreatmentAndLogLabelAndBucketingKeyForTreatment()
        {
            //Arrange
            var treatmentLogMock = new Mock<IImpressionListener>();
            var client = new JSONFileClient(@"Resources\splits_staging_3.json", "", null, null, treatmentLogMock.Object);

            //Act           
            var key = new Key("db765170-e9f2-11e5-885c-c2f58c3a47a7", "ab765170-e9f2-11e5-885c-c2f58c3a47a7");
            var result = client.GetTreatment(key, "Segments_Restructuring_UI", null);

            //Assert
            treatmentLogMock.Verify(x => x.Log(It.Is<KeyImpression>(p => p.keyName == "db765170-e9f2-11e5-885c-c2f58c3a47a7" && p.feature == "Segments_Restructuring_UI" && p.treatment == "on" && p.time > 0 && p.changeNumber == 1484084207827 && p.label == "explicitly included" && p.bucketingKey == "ab765170-e9f2-11e5-885c-c2f58c3a47a7")));

        }
    }
}
