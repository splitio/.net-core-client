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

using System;

namespace Splitio.Services.Cache.Lru
{
    /// <summary>
    /// Node for storing data in the doubly linked list
    /// </summary>
    /// <typeparam name="TKey">Type of key to use to identify the data</typeparam>
    /// <typeparam name="TValue">Type of data to store in the cache</typeparam>
    public sealed class Node<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node{TKey,TValue}"/> class
        /// </summary>
        /// <param name="key">Key for the node</param>
        /// <param name="data">Data for the node</param>
        public Node(TKey key, TValue data)
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null", nameof(key));
            }
            if (data == null)
            {
                throw new ArgumentException("Data cannot be null", nameof(data));
            }

            Value = data;
            Key = key;
            Next = null;
            Previous = null;
        }

        /// <summary>
        /// Gets or sets the data stored in the node
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Gets or sets the key for the data in the cache
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Gets or sets the next node in the list
        /// </summary>
        public Node<TKey, TValue> Next { get; set; }

        /// <summary>
        /// Gets or sets the previous node in the list
        /// </summary>
        public Node<TKey, TValue> Previous { get; set; }

        public override string ToString()
        {
            return $"Key:{Key} Data:{Value.ToString()} Previous:{GetNodeSummary(Previous)} Next:{GetNodeSummary(Next)}";
        }

        private string GetNodeSummary(Node<TKey, TValue> node)
        {
            return node != null ? "Set" : "Null";
        }
    }
}
