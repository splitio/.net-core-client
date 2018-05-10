using Common.Logging;
using Moq;
using Splitio.Services.Client.Classes;
using Xunit;

namespace Splitio.TestSupport.Samples
{
    public class SampleTest
    {
        private SplitClientForTest splitClient;
        private Mock<ILog> _logMock = new Mock<ILog>();

        [Theory]
        [SplitTest(test: @"{ feature:'feature_reads', treatments:['on', 'dark', 'off'] }")]
        public void SampleTest1(string feature, string treatment)
        {
            //Arrange
            splitClient = new SplitClientForTest(_logMock.Object);
            splitClient.RegisterTreatment(feature, treatment);

            //Act
            var actual = splitClient.GetTreatment("test", feature);

            //Assert
            Assert.Equal(treatment, actual);
        }

        [Theory]
        [SplitScenario(features:
         @"[
            { feature:'feature_reads', treatments:['on', 'dark', 'off'] },
            { feature:'feature_reads2', treatments:['on2', 'dark2', 'off2'] }
            ]")]
        public void SampleTest2(string feature, string treatment)
        {
            //Arrange
            splitClient = new SplitClientForTest(_logMock.Object);
            splitClient.RegisterTreatment(feature, treatment);

            //Act
            var actual = splitClient.GetTreatment("test", feature);

            //Assert
            Assert.Equal(treatment, actual);
        }

        [Theory]
        [SplitSuite(scenarios:
         @"[{
               features:
               [{ feature:'feature_reads', treatments:['on', 'dark', 'off'] },
                { feature:'feature_reads2', treatments:['on2', 'dark2', 'off2'] }]
            },
            {  features:
               [{ feature:'feature_reads', treatments:['on', 'dark', 'off'] },
                { feature:'feature_reads2', treatments:['on2', 'dark2', 'off2'] }]
            }]")]
        public void SampleTest3(string feature, string treatment)
        {
            //Arrange
            splitClient = new SplitClientForTest(_logMock.Object);
            splitClient.RegisterTreatment(feature, treatment);

            //Act
            var actual = splitClient.GetTreatment("test", feature);

            //Assert
            Assert.Equal(treatment, actual);
        }


    }
}
