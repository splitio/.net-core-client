using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Cache.Classes;
using System.Collections.Concurrent;
using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio_net_frameworks_tests.Unit_Tests.Cache
{
    [TestClass]
    public class SegmentCacheTests
    {
        [TestMethod]
        public void RegisterSegmentTest()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var keys = new List<string> { "abcd", "1234" };
            var segmentName = "test";

            //Act
            segmentCache.AddToSegment(segmentName, keys);
            var result = segmentCache.IsInSegment(segmentName, "abcd");
            
            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsNotInSegmentTest()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var keys = new List<string> { "1234" };
            var segmentName = "test";

            //Act
            segmentCache.AddToSegment(segmentName, keys);
            var result = segmentCache.IsInSegment(segmentName, "abcd");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInSegmentWithInexistentSegmentTest()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());

            //Act
            var result = segmentCache.IsInSegment("test", "abcd");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RemoveKeyFromSegmentTest()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var keys = new List<string> { "1234" };
            var segmentName = "test";

            //Act
            segmentCache.AddToSegment(segmentName, keys);
            var result = segmentCache.IsInSegment(segmentName, "1234");
            segmentCache.RemoveFromSegment(segmentName, keys);
            var result2 = segmentCache.IsInSegment(segmentName, "1234");

            //Assert
            Assert.IsTrue(result);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void SetAndGetChangeNumberTest()
        {
            //Arrange
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            var segmentName = "test";

            //Act
            segmentCache.AddToSegment(segmentName, null);
            segmentCache.SetChangeNumber(segmentName, 1234);
            var result = segmentCache.GetChangeNumber(segmentName);

            //Assert
            Assert.AreEqual(1234, result);
        }
    }
}
