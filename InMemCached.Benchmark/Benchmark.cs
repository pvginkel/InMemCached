using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace InMemCached.Benchmark
{
    internal class Benchmark<T>
        where T : IContainer, new()
    {
        private volatile bool _stopped;
        private int _adds;
        private int _removes;
        private int _gets;

        public void Run(int maxKey, int valueSize, int iterations)
        {
            using (var container = new T())
            {
                var threads = new List<Thread>();
                int threadCount = Environment.ProcessorCount;

                Console.WriteLine("Seeding cache");

                var memory = Enumerable.Range(0, valueSize).Select(p => (byte)p).ToArray();

                for (int i = 0; i < maxKey; i += 2)
                {
                    container.Add(i, memory);
                }

                Console.WriteLine($"Starting {threadCount} threads");

                for (int i = 0; i < threadCount; i++)
                {
                    var thread = new Thread(p => ThreadProc((T)p, maxKey, valueSize));
                    thread.Start(container);
                    threads.Add(thread);
                }

                for (int i = 0; i < iterations; i++)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    Report(i + 1);
                }

                Console.WriteLine("Stopping");

                _stopped = true;

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }

            Report(0);
        }

        private void ThreadProc(T container, int maxKey, int valueSize)
        {
            var random = new Random();
            var memory = Enumerable.Range(0, valueSize).Select(p => (byte)p).ToArray();

            while (!_stopped)
            {
                if (container.Get(random.Next(maxKey)) != null)
                    Interlocked.Increment(ref _gets);

                if (random.Next(100) == 0)
                {
                    if (container.Add(random.Next(maxKey), memory))
                        Interlocked.Increment(ref _adds);

                    if (container.Remove(random.Next(maxKey)))
                        Interlocked.Increment(ref _removes);
                }
            }
        }

        private void Report(int iteration)
        {
            Console.WriteLine($"Iteration {iteration} adds {_adds}, gets {_gets}, removes {_removes}");
        }
    }
}
