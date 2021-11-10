using BenchmarkDotNet.Running;

namespace Mandible.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<InflateBenchmarks>(args: args);
    }
}
