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
using System.Collections;
using System.Collections.Generic;

namespace Splitio.Services.Cache.Lru
{
    /// <summary>
    /// Enumator for iterating through a list of <see cref="Node{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Key for the items in the nodes</typeparam>
    /// <typeparam name="TValue">Value for the items in the nodes</typeparam>
    public class CacheEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private Node<TKey, TValue> _head;
        private Node<TKey, TValue> _current;
        private LruCache<TKey, TValue> _cache;
        private bool _collectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheEnumerator{TKey, TValue}"/> class
        /// </summary>
        /// <param name="cache">Cache to which this enumerator belongs</param>
        /// <param name="head">First item in the list</param>
        public CacheEnumerator(LruCache<TKey, TValue> cache, Node<TKey, TValue> head)
        {
            if (cache != null)
            {
                _cache = cache;
                cache.CollectionChanged += Cache_CollectionChanged;
            }

            _head = head ?? throw new ArgumentException("Head can't be null");
            _current = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheEnumerator{TKey, TValue}"/> class
        /// </summary>
        /// <param name="head">First item in the list</param>
        public CacheEnumerator(Node<TKey, TValue> head) : this(null, head)
        {
        }

        /// <summary>
        /// Gets the current item in the enumerator
        /// </summary>
        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                if (_head == null)
                {
                    throw new ObjectDisposedException(nameof(CacheEnumerator<TKey, TValue>));
                }
                if (_collectionChanged)
                {
                    throw new InvalidOperationException("Collection has changed");
                }

                return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
            }
        }

        /// <summary>
        /// Gets the current item in the enumerator
        /// </summary>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Clears the items in the enumerator
        /// </summary>
        public void Dispose()
        {
            _head = null;
            _current = null;
            if (_cache != null)
            {
                _cache.CollectionChanged -= Cache_CollectionChanged;
            }
            _cache = null;
        }

        /// <summary>
        /// Tries to move to the next item in the enumerator
        /// </summary>
        /// <returns>True if successfully moved to the next item, false if there were no more items</returns>
        public bool MoveNext()
        {
            if (_head == null)
            {
                throw new ObjectDisposedException(nameof(CacheEnumerator<TKey, TValue>));
            }
            if (_collectionChanged)
            {
                throw new InvalidOperationException("Collection has changed");
            }

            if (_current == null)
            {
                _current = _head;
                return true;
            }

            if (_current.Next != null)
            {
                _current = _current.Next;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the enumerator back to the head item in the list
        /// </summary>
        public void Reset()
        {
            if (_collectionChanged)
            {
                throw new InvalidOperationException("Collection has changed");
            }

            _current = null;
        }

        /// <summary>
        /// Fired when the collection containing this enumerator changes
        /// </summary>
        /// <param name="sender">Cache that changed</param>
        /// <param name="e">Event args</param>
        private void Cache_CollectionChanged(object sender, EventArgs e)
        {
            _collectionChanged = true;
        }
    }
}
