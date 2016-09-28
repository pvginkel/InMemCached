using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InMemCached.Benchmark
{
    public static class Program
    {
        static void Main(string[] args)
        {
            new Benchmark<InMemContainer>().Run(20000, 100000, 30);
        }
    }
}
