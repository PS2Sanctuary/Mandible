using BenchmarkDotNet.Attributes;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Mandible.Benchmarks;

[MemoryDiagnoser]
public class RandomBenchmarks
{
    private readonly byte[] _data = Encoding.ASCII.GetBytes("ThisIsAString");

    [Benchmark]
    public string EncodingRead()
    {
        return Encoding.ASCII.GetString(_data);
    }

    [Benchmark]
    public unsafe string? MarshalRead()
    {
        fixed (byte* bufferPtr = _data)
        {
            return Marshal.PtrToStringAnsi((IntPtr)bufferPtr);
        }
    }
}
