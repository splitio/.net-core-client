// Copyright (c) Mark Davis. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Cache.Lru;
using System.Collections.Generic;

namespace Splitio_Tests.Unit_Tests.Cache.Lru
{
    [TestClass]
    public class IDictionaryTests
    {
        /// <summary>
        /// Tests that a handful of IDictionary interface methods work
        /// </summary>
        [TestMethod, TestCategory("IDictionary")]
        public void DictionaryTests()
        {
            IDictionary<int, int> data = new LruCache<int, int>(10);
            data[0] = 1;
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(1, data[0]);
            Assert.AreEqual(1, data.Keys.Count);
            Assert.AreEqual(1, data.Values.Count);
        }
    }
}
