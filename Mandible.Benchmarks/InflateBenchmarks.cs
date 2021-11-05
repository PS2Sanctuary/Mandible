using BenchmarkDotNet.Attributes;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Mandible.Zng.Inflate;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Mandible.Benchmarks
{
    [MemoryDiagnoser]
    public class InflateBenchmarks
    {
        private readonly Inflater _inflater;
        private readonly ZngInflater _zngInflater;

        [AllowNull]
        private byte[] _deflatedData;

        [AllowNull]
        private int _inflatedLength;

        public InflateBenchmarks()
        {
            _inflater = new Inflater();
            _zngInflater = new ZngInflater();
        }

        [GlobalSetup]
        [MemberNotNull(nameof(_deflatedData), nameof(_inflatedLength))]
        public void GlobalSetup()
        {
            using MemoryStream ms = new();
            using FileStream fs = new("Data\\icon_TR_Plasma2_PS_32.dds", FileMode.Open);

            fs.CopyTo(ms);
            _inflatedLength = (int)ms.Length;

            byte[] input = new byte[_inflatedLength];
            ms.Read(input);

            byte[] output = new byte[_inflatedLength];
            Deflater deflater = new(Deflater.BEST_COMPRESSION);

            deflater.SetInput(input);
            deflater.Finish();

            int amountDeflated = deflater.Deflate(output);
            _deflatedData = output[0..amountDeflated];
        }

        [Benchmark]
        public ReadOnlySpan<byte> SharpZipLib()
        {
            _inflater.SetInput(_deflatedData);

            byte[] output = new byte[_inflatedLength];
            _inflater.Inflate(output);

            _inflater.Reset();
            return output;
        }

        [Benchmark(Baseline = true)]
        public ReadOnlySpan<byte> ZlibNG()
        {
            byte[] output = new byte[_inflatedLength];
            _zngInflater.Inflate(_deflatedData, output);

            _zngInflater.Reset();
            return output;
        }

        [Benchmark]
        public ReadOnlySpan<byte> ZlibNGConstructAndDispose()
        {
            byte[] output = new byte[_inflatedLength];
            using ZngInflater inflater = new();

            inflater.Inflate(_deflatedData, output);
            return output;
        }
    }
}
