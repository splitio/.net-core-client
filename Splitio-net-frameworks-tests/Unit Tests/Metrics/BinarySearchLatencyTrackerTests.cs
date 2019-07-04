using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Metrics.Classes;
using System.Collections.Generic;
using System.Linq;

namespace Splitio_net_frameworks_tests.Unit_Tests.Metrics
{
    [TestClass]
    public class BinarySearchLatencyTrackerTests
    {
        [TestMethod]
        public void FindIndexShouldReturnValidOutput()
        {
            //Arrange
            List<int> valuesToTest = new List<int>()
                                 {
                                  0, 90, 1000,     
                                  1028, 1500,    
                                  1879, 2250,   
                                  2789, 3375,    
                                  4098, 5063,
                                  6060, 7594,    
                                  10999, 11391,   
                                  15000, 17086,  
                                  22234, 25629,   
                                  30000, 38443,
                                  40404, 57665,   
                                  60999, 86498,   
                                  100000, 129746, 
                                  150000, 194620,  
                                  200000, 291929,
                                  409999, 437894,  
                                  500000, 656841,  
                                  780909, 985261, 
                                  1200000, 1477892, 
                                  2000000, 2216838,
                                  3000000, 3325257, 
                                  3500000, 4987885, 
                                  5000000, 7481828, 8888899
                                 };

            var bslt = new BinarySearchLatencyTracker();

            //Act
            List<int> result = valuesToTest.Select(x => bslt.FindIndex(x)).ToList();

            //Assert
            Assert.AreEqual(0, result[0]);
            Assert.AreEqual(0, result[1]);
            Assert.AreEqual(0, result[2]);
            Assert.AreEqual(1, result[3]);
            Assert.AreEqual(1, result[4]);
            Assert.AreEqual(2, result[5]);
            Assert.AreEqual(2, result[6]);
            Assert.AreEqual(3, result[7]);
            Assert.AreEqual(3, result[8]);
            Assert.AreEqual(4, result[9]);
            Assert.AreEqual(4, result[10]);
            Assert.AreEqual(5, result[11]);
            Assert.AreEqual(5, result[12]);
            Assert.AreEqual(6, result[13]);
            Assert.AreEqual(6, result[14]);
            Assert.AreEqual(7, result[15]);
            Assert.AreEqual(7, result[16]);
            Assert.AreEqual(8, result[17]);
            Assert.AreEqual(8, result[18]);
            Assert.AreEqual(9, result[19]);
            Assert.AreEqual(9, result[20]);
            Assert.AreEqual(10, result[21]);
            Assert.AreEqual(10, result[22]);
            Assert.AreEqual(11, result[23]);
            Assert.AreEqual(11, result[24]);
            Assert.AreEqual(12, result[25]);
            Assert.AreEqual(12, result[26]);
            Assert.AreEqual(13, result[27]);
            Assert.AreEqual(13, result[28]);
            Assert.AreEqual(14, result[29]);
            Assert.AreEqual(14, result[30]);
            Assert.AreEqual(15, result[31]);
            Assert.AreEqual(15, result[32]);
            Assert.AreEqual(16, result[33]);
            Assert.AreEqual(16, result[34]);
            Assert.AreEqual(17, result[35]);
            Assert.AreEqual(17, result[36]);
            Assert.AreEqual(18, result[37]);
            Assert.AreEqual(18, result[38]);
            Assert.AreEqual(19, result[39]);
            Assert.AreEqual(19, result[40]);
            Assert.AreEqual(20, result[41]);
            Assert.AreEqual(20, result[42]);
            Assert.AreEqual(21, result[43]);
            Assert.AreEqual(21, result[44]);
            Assert.AreEqual(22, result[45]);
            Assert.AreEqual(22, result[46]);
            Assert.AreEqual(22, result[47]);

        }
    }
}
