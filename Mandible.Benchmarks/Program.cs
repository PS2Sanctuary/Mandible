using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Mandible.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<AssetReadBenchmarks>();
        }
    }
}
