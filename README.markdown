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

## Performance

A benchmark is available as part of the project. Below are some performance statistics gathered
using this benchmark. The benchmark was configured as follows:

* CPU: Intel Xeon E3-1245 v3 @ 3.4 GHz (VM);
* Threads/cores: 4;
* Maximum cache entries: 20,000 (averages at half);
* Cache entry size: 100 Kb;
* Runtime: 30 seconds.

The benchmark was run on a naive managed implementation using just a `ConcurrentDictionary<int, byte[]>`
and two versions of the `InMemCache`: one with object pooling and one without.

The result are as follows:

* Managed: 45435 retrievals per second;
* InMem: 28089 retrievals per second;
* InMem pooled: 28610 retrievals per second.

Using PerfMon, the following GC statistics were gathered:

| Run | G0 | G1 | G2 | Max pause | Total pause | Peak GC size | Peak working set |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Managed | 220 | 138 | 144 | 61.6 ms | 1.1 s | 2,490 Mb | 2,514 Mb |
| InMem | 0 | 0 | 15109 | 8.9 ms | 13.7 s | 5 Mb | 1,033 Mb |
| InMem pooled | 0 | 0 | 6165 | 3.4 ms | 6.3 s | 5 Mb | 1,025 Mb |

The difference in G2 collections between the pooled and non pooled version probably can
be explained by the removal of a finalizer. In the non pooled version, references to
native memory were managed using a finalizer, which requires the collection of an object
to be delayed, even if the finalizer is not run. In the pooled version, no finalizer is
used. The reason this is not a problem is that all allocations are done from a single heap
which is destroyed when the `InMemCache` instance is disposed. This ensures that we don't
leak memory even if we don't free all separate allocations.

The full details are as follows:

* Managed:

![Managed GC stats](https://raw.githubusercontent.com/pvginkel/InMemCached/master/Documentation/Managed%20GC%20stats.png)

* InMem:

![InMem GC stats](https://raw.githubusercontent.com/pvginkel/InMemCached/master/Documentation/InMem%20naive%20GC%20stats.png)

* InMem pooled:

![InMem pooled GC stats](https://raw.githubusercontent.com/pvginkel/InMemCached/master/Documentation/InMem%20pooled%20GC%20stats.png)

And the full PerfMon statistics:

* Managed:

![Managed GC PerfMon](https://raw.githubusercontent.com/pvginkel/InMemCached/master/Documentation/Managed%20PerfMon%20stats.png)

* InMem:

![InMem GC PerfMon](https://raw.githubusercontent.com/pvginkel/InMemCached/master/Documentation/InMem%20naive%20PerfMon%20stats.png)

* InMem pooled:

![InMem pooled GC PerfMon](https://raw.githubusercontent.com/pvginkel/InMemCached/master/Documentation/InMem%20pooled%20PerfMon%20stats.png)

## Bugs

Bugs should be reported through github at
[http://github.com/pvginkel/InMemCached/issues](http://github.com/pvginkel/InMemCached/issues).

## License

InMemCached is licensed under the Apache 2.0 license.
