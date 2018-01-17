using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Cache.Classes;
using Splitio.Domain;
using Splitio.Services.Impressions.Classes;

namespace Splitio_Tests.Unit_Tests.Cache
{
    [TestClass]
    public class ImpressionsCacheTests
    {
        [TestMethod]
        public void AddImpressionSuccessfully()
        {
            //Arrange
            var queue = new BlockingQueue<KeyImpression>(2);
            var cache = new InMemorySimpleCache<KeyImpression>(queue);
            
            //Act
            cache.AddItem(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });
            var element = queue.Dequeue();
            var element2 = queue.Dequeue();

            //Assert
            Assert.IsNotNull(element);
            Assert.IsNull(element2);
        }

        [TestMethod]
        public void AddImpressionWithFullQueue()
        {
            //Arrange
            var queue = new BlockingQueue<KeyImpression>(1);
            var cache = new InMemorySimpleCache<KeyImpression>(queue);

            //Act
            cache.AddItem(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });
            cache.AddItem(new KeyImpression() { feature = "test2", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });

            var element = queue.Dequeue();
            var element2 = queue.Dequeue();

            //Assert
            Assert.IsNotNull(element);
            Assert.IsNull(element2);
        }

        [TestMethod]
        public void FetchAllAndClearSuccessfully()
        {
            //Arrange
            var queue = new BlockingQueue<KeyImpression>(2);
            var cache = new InMemorySimpleCache<KeyImpression>(queue);
            cache.AddItem(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });
            cache.AddItem(new KeyImpression() { feature = "test2", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });

            //Act
            var result = cache.FetchAllAndClear();
            var element = queue.Dequeue();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsNull(element);
        }

        [TestMethod]
        public void FetchAllAndClearWithNullQueue()
        {
            //Arrange
            var cache = new InMemorySimpleCache<KeyImpression>(null);
            cache.AddItem(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });
            cache.AddItem(new KeyImpression() { feature = "test2", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });

            //Act
            var result = cache.FetchAllAndClear();

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void HasReachedMaxSizeSuccessfully()
        {
            //Arrange
            var queue = new BlockingQueue<KeyImpression>(1);
            var cache = new InMemorySimpleCache<KeyImpression>(queue);

            //Act
            cache.AddItem(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });
            cache.AddItem(new KeyImpression() { feature = "test2", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });

            var result = cache.HasReachedMaxSize();

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasReachedMaxSizeWithNullQueue()
        {
            //Arrange
            var queue = new BlockingQueue<KeyImpression>(3);
            var cache = new InMemorySimpleCache<KeyImpression>(queue);

            //Act
            cache.AddItem(new KeyImpression() { feature = "test", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });
            cache.AddItem(new KeyImpression() { feature = "test2", changeNumber = 100, keyName = "date", label = "testdate", time = 10000000 });

            var result = cache.HasReachedMaxSize();

            //Assert
            Assert.IsFalse(result);
        }

    }
}
