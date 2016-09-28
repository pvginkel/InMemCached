using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InMemCached.Benchmark
{
    internal class ManagedContainer : IContainer
    {
        private readonly ConcurrentDictionary<int, byte[]> _cache = new ConcurrentDictionary<int, byte[]>();

        public void Dispose()
        {
            // Nothing to do.
        }

        public bool Add(int key, byte[] value)
        {
            return _cache.TryAdd(key, (byte[])value.Clone());
        }

        public bool Contains(int key)
        {
            return _cache.ContainsKey(key);
        }

        public byte[] Get(int key)
        {
            byte[] value;
            if (_cache.TryGetValue(key, out value))
                return (byte[])value.Clone();
            return null;
        }

        public bool Remove(int key)
        {
            byte[] value;
            if (_cache.TryRemove(key, out value))
                return true;
            return false;
        }
    }
}
