using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InMemCached.Benchmark
{
    internal interface IContainer : IDisposable
    {
        bool Add(int key, byte[] value);
        bool Contains(int key);
        byte[] Get(int key);
        bool Remove(int key);
    }
}
