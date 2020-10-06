/*
https://github.com/mwdavis84/LruCacheNet
Copyright (c) 2018 Mark Davis

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Cache.Lru;
using System;

namespace Splitio_Tests.Unit_Tests.Cache.Lru
{
    [TestClass]
    public class NodeTests
    {
        [TestMethod, TestCategory("Node")]
        public void NullNodeKey()
        {
            bool thrown = false;
            try
            {
                var node = new Node<string, TestData>(null, new TestData());
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "ArgumentException not thrown");
        }

        [TestMethod, TestCategory("Node")]
        public void NullNodeValue()
        {
            bool thrown = false;
            try
            {
                var node = new Node<string, TestData>("0", null);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "ArgumentException not thrown");
        }

        [TestMethod, TestCategory("Node")]
        [DataRow(true, false, "Key:0 Data:0 Previous:Set Next:Null", DisplayName = "True|False")]
        [DataRow(false, true, "Key:0 Data:0 Previous:Null Next:Set", DisplayName = "False|True")]
        [DataRow(false, false, "Key:0 Data:0 Previous:Null Next:Null", DisplayName = "False|False")]
        [DataRow(true, true, "Key:0 Data:0 Previous:Set Next:Set", DisplayName = "True|True")]
        public void NodeToString(bool setPrevious, bool setNext, string expected)
        {
            var node = new Node<string, string>("0", "0");
            if (setPrevious)
            {
                node.Previous = new Node<string, string>("1", "1");
            }
            if (setNext)
            {
                node.Next = new Node<string, string>("2", "2");
            }
            string str = node.ToString();
            Assert.AreEqual(expected, str);
        }
    }
}
