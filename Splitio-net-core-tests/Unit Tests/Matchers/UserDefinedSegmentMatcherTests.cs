using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Parsing;
using Splitio.Domain;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Splitio.Services.Cache.Classes;

namespace Splitio_Tests.Unit_Tests
{
    [TestClass]
    public class UserDefinedSegmentMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingSegmentWithKey()
        {
            //Arrange
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test2");

            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            segmentCache.AddToSegment(segmentName, keys);

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match(new Key("test2", "test2"));

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingSegmentWithKey()
        {
            //Arrange
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test2");

            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            segmentCache.AddToSegment(segmentName, keys);

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match(new Key("test3", "test3"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfSegmentEmptyWithKey()
        {
            //Arrange
            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            segmentCache.AddToSegment(segmentName, null);

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match(new Key("test2", "test2"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfCacheEmptyWithKey()
        {
            //Arrange
            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match(new Key("test2", "test2"));

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingSegment()
        {
            //Arrange
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test2");

            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            segmentCache.AddToSegment(segmentName, keys);

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match("test2");

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingSegment()
        {
            //Arrange
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test2");

            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            segmentCache.AddToSegment(segmentName, keys);

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match("test3");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfSegmentEmpty()
        {
            //Arrange
            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());
            segmentCache.AddToSegment(segmentName, null);

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match("test2");

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfCacheEmpty()
        {
            //Arrange
            var segmentName = "test-segment";
            var segmentCache = new InMemorySegmentCache(new ConcurrentDictionary<string, Segment>());

            var matcher = new UserDefinedSegmentMatcher(segmentName, segmentCache);

            //Act
            var result = matcher.Match("test2");

            //Assert
            Assert.IsFalse(result);
        }
    }
}
