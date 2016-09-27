# InMemCached

Apache 2.0 License.

[Download from NuGet](http://nuget.org/packages/InMemCached).

## Introduction

**Please note that this is something I threw together based on the blog
post referred to below. This is not something that has been thoroughly tested
in a production environment, either for stability or even usefullness. If
this is something that is useful for you, please let me know.**

InMemCached is a library to provide in process caching using the Windows native
heap.

This library functions similar to how MemCached works, except that it does so
in process instead of out of process.

The functionality provided by this library could be implemented using a simple
`ConcurrentDictionary<TKey, byte[]>`. However, this library uses the native
Windows heap instead of the GC heap. The advantage of this is that this release
pressure on the garbage collector.

This library is specifically useful for when you have a cache of objects that either
are too large for the normal GC heap and thus are stored on the large object heap
(i.e. over 85K in size), or when you have lots of larger objects that generally
reach the Gen 2 heap, but then are released. See
[When a disk cache performs better than an in-memory cache (befriending the .net GC)](http://www.productiverage.com/when-a-disk-cache-performs-better-than-an-inmemory-cache-befriending-the-net-gc)
by Dan Roberts for a nice war story on such issues.

## Usage

Basically the `InMemCache` class wraps a `ConcurrentDictionary<TKey, ...>` and manages
allocation and deallocation of memory for you. You can provide your own key type.

```cs
using (var cache = new InMemCache<int>())
{
    cache.Add(42, new byte[] { 1, 2, 3 });
    
    var bytes = cache.Get(42);
    
    cache.Remove(42);
}
```

## Implementation details

Every `InMemCache` instance has its own native heap as allocated by
[`HeapCreate`](https://msdn.microsoft.com/en-us/library/windows/desktop/aa366599.aspx).
These heaps are marked as low fragmentation heaps.

All methods (except `Dispose`) on the class are thread safe.

## Bugs

Bugs should be reported through github at
[http://github.com/pvginkel/InMemCached/issues](http://github.com/pvginkel/InMemCached/issues).

## License

InMemCached is licensed under the Apache 2.0 license.
