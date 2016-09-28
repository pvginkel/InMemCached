using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace InMemCached
{
    public class InMemCache<T> : IDisposable
    {
        private static readonly MemoryHandle Tail = new MemoryHandle();

        private HeapHandle _heap;
        private ConcurrentDictionary<T, MemoryHandle> _cache = new ConcurrentDictionary<T, MemoryHandle>();
        private volatile MemoryHandle _pool;
        private bool _disposed;

        public InMemCache()
        {
            _heap = new HeapHandle();
            _pool = Tail;
        }

        public int Count => _cache.Count;

        private MemoryHandle GetMemoryHandle()
        {
            while (true)
            {
                var result = _pool;

                Debug.Assert(result != null);

                if (result == Tail)
                    return new MemoryHandle();

                if (result == Interlocked.CompareExchange(ref _pool, result.next, result))
                {
                    result.next = null;
                    return result;
                }
            }
        }

        private void ReleaseMemoryHandle(MemoryHandle handle)
        {
            while (true)
            {
                var pool = _pool;

                Debug.Assert(pool != null);

                handle.next = pool;

                if (pool == Interlocked.CompareExchange(ref _pool, handle, pool))
                    return;
            }
        }

        public bool Add(T key, byte[] value)
        {
            var handle = GetMemoryHandle();

            handle.Alloc(_heap, value);

            if (_cache.TryAdd(key, handle))
                return true;

            handle.Free(_heap);

            ReleaseMemoryHandle(handle);

            return false;
        }

        public byte[] Get(T key)
        {
            MemoryHandle handle;
            if (_cache.TryGetValue(key, out handle))
            {
                lock (handle)
                {
                    return handle.ToByteArray();
                }
            }

            return null;
        }

        public bool Remove(T key)
        {
            MemoryHandle handle;
            if (_cache.TryRemove(key, out handle))
            {
                lock (handle)
                {
                    handle.Free(_heap);
                }

                ReleaseMemoryHandle(handle);

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
                    foreach (var handle in _cache.Values)
                    {
                        lock (handle)
                        {
                            handle.Free(_heap);
                        }
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

        private class MemoryHandle
        {
            public MemoryHandle next;
            private IntPtr _handle;
            private int _length;

            public void Alloc(HeapHandle heap, byte[] value)
            {
                Debug.Assert(_handle == IntPtr.Zero);

                _length = value.Length;
                _handle = NativeMethods.HeapAlloc(heap, 0, (IntPtr)_length);

                Marshal.Copy(value, 0, _handle, value.Length);
            }

            public void Free(HeapHandle heap)
            {
                Debug.Assert(_handle != IntPtr.Zero);

                NativeMethods.HeapFree(heap, 0, _handle);

                _handle = IntPtr.Zero;
            }

            public byte[] ToByteArray()
            {
                var result = new byte[_length];

                if (_handle == IntPtr.Zero)
                    return null;

                Marshal.Copy(_handle, result, 0, _length);

                return result;
            }
        }
    }
}
