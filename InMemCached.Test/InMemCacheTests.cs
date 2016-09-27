using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace InMemCached.Test
{
    [TestFixture]
    public class InMemCacheTests
    {
        [Test]
        public void SimpleAdd()
        {
            using (var cache = new InMemCache<int>())
            {
                Assert.IsTrue(cache.Add(1, new byte[] { 1 }));
            }
        }

        [Test]
        public void AddAndRetrieve()
        {
            using (var cache = new InMemCache<int>())
            {
                Assert.IsTrue(cache.Add(1, new byte[] { 1 }));

                var bytes = cache.Get(1);

                Assert.NotNull(bytes);
                Assert.AreEqual(1, bytes.Length);
                Assert.AreEqual(1, bytes[0]);
            }
        }

        [Test]
        public void Contains()
        {
            using (var cache = new InMemCache<int>())
            {
                Assert.IsTrue(cache.Add(1, new byte[] { 1 }));

                Assert.IsTrue(cache.Contains(1));
            }
        }

        [Test]
        public void AddAndRemove()
        {
            using (var cache = new InMemCache<int>())
            {
                Assert.IsTrue(cache.Add(1, new byte[] { 1 }));
                Assert.IsTrue(cache.Contains(1));
                Assert.IsTrue(cache.Remove(1));
                Assert.IsFalse(cache.Contains(1));
                Assert.IsFalse(cache.Remove(1));
            }
        }

        [Test]
        public void MultiThreadedAdd()
        {
            long usage = GetMemoryUsage();

            using (var cache = new InMemCache<int>())
            {
                int nextIndex = 0;
                var memory = Enumerable.Range(0, 20 * 1024).Select(p => (byte)p).ToArray();
                var threads = new List<Thread>();

                for (int i = 0; i < 10; i++)
                {
                    var thread = new Thread(p =>
                    {
                        for (int j = 0; j < 10000; j++)
                        {
                            int index = Interlocked.Increment(ref nextIndex);
                            Assert.IsTrue(cache.Add(index, memory));
                        }
                    });

                    thread.Start();

                    threads.Add(thread);
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }

                long currentUsage = GetMemoryUsage() - usage;

                Console.WriteLine($"After storing {cache.Count} of {NiceMemory(memory.Length)} entries: {NiceMemory(currentUsage)}, per entry {NiceMemory(currentUsage / cache.Count)}");

                for (int i = 0; i < nextIndex; i++)
                {
                    Assert.IsTrue(cache.Contains(i + 1));
                    Assert.IsTrue(cache.Remove(i + 1));
                }

                Console.WriteLine($"After removing entries: {NiceMemory(GetMemoryUsage() - usage)}");
            }

            Console.WriteLine($"After disposing cache: {NiceMemory(GetMemoryUsage() - usage)}");
        }

        private static long GetMemoryUsage()
        {
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();

            return GC.GetTotalMemory(false);
        }

        private static string NiceMemory(long bytes)
        {
            if (bytes > 1024)
            {
                double value = bytes / 1024.0;
                if (value > 1024)
                {
                    value /= 1024;

                    if (value > 1024)
                    {
                        value /= 1024;

                        return value.ToString("0.0") + " Gb";
                    }
                    else
                    {
                        return value.ToString("0.0") + " Mb";
                    }
                }
                else
                {
                    return value.ToString("0.0") + " Kb";
                }
            }
            else
            {
                return bytes + " b";
            }
        }
    }
}
