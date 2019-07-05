using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using System.Collections.Concurrent;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class SplitCacheTests
    {

        [TestMethod]
        public void AddAndGetSplitTest()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var splitName = "test1";

            //Act
            splitCache.AddSplit(splitName, new ParsedSplit() { name = splitName });
            var result = splitCache.GetSplit(splitName);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AddDuplicateSplitTest()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var splitName = "test1";

            //Act
            var parsedSplit1 = new ParsedSplit() { name = splitName };
            splitCache.AddSplit(splitName, parsedSplit1);
            var parsedSplit2 = new ParsedSplit() { name = splitName };
            splitCache.AddSplit(splitName, parsedSplit2);
            var result = splitCache.GetAllSplits();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(result[0], parsedSplit1);
            Assert.AreNotEqual(result[0], parsedSplit2);
        }

        [TestMethod]
        public void GetInexistentSplitTest()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var splitName = "test1";

            //Act
            var result = splitCache.GetSplit(splitName);

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void RemoveSplitTest()
        {
            //Arrange
            var splitName = "test1";
            var splits = new ConcurrentDictionary<string, ParsedSplit>();
            splits.TryAdd(splitName, new ParsedSplit() { name = splitName });
            var splitCache = new InMemorySplitCache(splits);
            
            //Act
            splitCache.RemoveSplit(splitName);
            var result = splitCache.GetSplit(splitName);

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void SetAndGetChangeNumberTest()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var changeNumber = 1234;

            //Act
            splitCache.SetChangeNumber(changeNumber);
            var result = splitCache.GetChangeNumber();

            //Assert
            Assert.AreEqual(changeNumber, result);
        }

        [TestMethod]
        public void GetAllSplitsTest()
        {
            //Arrange
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());
            var splitName = "test1";
            var splitName2 = "test2";

            //Act
            splitCache.AddSplit(splitName, new ParsedSplit() { name = splitName });
            splitCache.AddSplit(splitName2, new ParsedSplit() { name = splitName2 });

            var result = splitCache.GetAllSplits();

            //Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void AddOrUpdate_WhenUpdateTraffictType_ReturnsTrue()
        {
            // Arrange 
            var splitCache = new InMemorySplitCache(new ConcurrentDictionary<string, ParsedSplit>());

            var splitName = "split_1";
            var splitName2 = "split_2";

            var split = new ParsedSplit { name = splitName, trafficTypeName = "traffic_type_1" };
            var split2 = new ParsedSplit { name = splitName, trafficTypeName = "traffic_type_2" };
            var split3 = new ParsedSplit { name = splitName, trafficTypeName = "traffic_type_3" };
            var split4 = new ParsedSplit { name = splitName2, trafficTypeName = "traffic_type_4" };

            splitCache.AddOrUpdate(splitName, split);
            splitCache.AddOrUpdate(splitName, split2);
            splitCache.AddOrUpdate(splitName, split3);
            splitCache.AddOrUpdate(splitName2, split4);

            // Act
            var result1 = splitCache.TrafficTypeExists("traffic_type_1");
            var result2 = splitCache.TrafficTypeExists("traffic_type_2");
            var result3 = splitCache.TrafficTypeExists("traffic_type_3");

            // Assert
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
            Assert.IsTrue(result3);
        }
    }
}
