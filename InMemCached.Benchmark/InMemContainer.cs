using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InMemCached.Benchmark
{
    internal class InMemContainer : IContainer
    {
        private InMemCache<int> _cache = new InMemCache<int>();
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_cache != null)
                {
                    _cache.Dispose();
                    _cache = null;
                }

                _disposed = true;
            }
        }

        public bool Add(int key, byte[] value)
        {
            return _cache.Add(key, value);
        }

        public bool Contains(int key)
        {
            return _cache.Contains(key);
        }

        public byte[] Get(int key)
        {
            return _cache.Get(key);
        }

        public bool Remove(int key)
        {
            return _cache.Remove(key);
        }
    }
}
