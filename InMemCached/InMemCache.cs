using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InMemCached
{
    public class InMemCache<T> : IDisposable
    {
        private HeapHandle _heap;
        private ConcurrentDictionary<T, MemoryHandle> _cache = new ConcurrentDictionary<T, MemoryHandle>();
        private bool _disposed;

        public InMemCache()
        {
            _heap = new HeapHandle();
        }

        public int Count => _cache.Count;

        public bool Add(T key, byte[] value)
        {
            var handle = new MemoryHandle(_heap, value);

            if (_cache.TryAdd(key, handle))
                return true;

            handle.Dispose();

            return false;
        }

        public byte[] Get(T key)
        {
            MemoryHandle handle;
            if (_cache.TryGetValue(key, out handle))
                return handle.ToByteArray();

            return null;
        }

        public bool Remove(T key)
        {
            MemoryHandle handle;
            if (_cache.TryRemove(key, out handle))
            {
                handle.Dispose();

                return true;
            }

            return false;
        }

        public bool Contains(T key)
        {
            return _cache.ContainsKey(key);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_cache != null)
                {
                    foreach (var entry in _cache.Values)
                    {
                        entry.Dispose();
                    }

                    _cache = null;
                }

                if (_heap != null)
                {
                    _heap.Dispose();
                    _heap = null;
                }

                _disposed = true;
            }
        }
    }
}
