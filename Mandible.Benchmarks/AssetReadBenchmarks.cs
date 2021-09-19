using BenchmarkDotNet.Attributes;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Mandible.Benchmarks
{
    public class AssetReadBenchmarks
    {
        private const uint COMPRESSION_INDICATOR = 2712847316;
        private const int DATA_LENGTH = 102400;

        private readonly Inflater _inflater;

        [AllowNull]
        private string _dataFile;

        [AllowNull]
        private SafeFileHandle _fileHandle;
        [AllowNull]
        private FileStream _fileStream;

        public AssetReadBenchmarks()
        {
            _inflater = new Inflater();
        }

        [GlobalSetup]
        [MemberNotNull(nameof(_dataFile))]
        public void GlobalSetup()
        {
            byte[] data = new byte[DATA_LENGTH];

            for (int i = 0; i < DATA_LENGTH; i++)
                data[i] = (byte)i;

            _dataFile = Path.GetTempFileName();
            SafeFileHandle handle = File.OpenHandle(_dataFile, FileMode.Open, FileAccess.Write, FileShare.None);

            RandomAccess.Write(handle, data, 0);

            byte[] compressionInfo = new byte[8];
            BinaryPrimitives.WriteUInt32BigEndian(compressionInfo, COMPRESSION_INDICATOR);
            BinaryPrimitives.WriteUInt32BigEndian(compressionInfo.AsSpan(4, 4), DATA_LENGTH);
            RandomAccess.Write(handle, compressionInfo, DATA_LENGTH);

            Deflater deflater = new(Deflater.BEST_COMPRESSION);
            deflater.SetInput(data);

            deflater.Finish();

            int amountDeflated = deflater.Deflate(data);
            RandomAccess.Write(handle, data.AsSpan(0, amountDeflated), DATA_LENGTH + compressionInfo.Length);

            handle.Dispose();
            // TODO: Compare with differing FileShare
            _fileHandle = File.OpenHandle(_dataFile, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.Asynchronous | FileOptions.RandomAccess);
            _fileStream = new(_fileHandle, FileAccess.Read, DATA_LENGTH); // NOTE - we're using a perfect buffer size here.
        }

        [Benchmark]
        public async Task<ReadOnlyMemory<byte>> RandomAccessAsync()
        {
            // Read uncompressed block
            byte[] data = new byte[DATA_LENGTH];
            Memory<byte> dataMem = new(data);

            await RandomAccess.ReadAsync(_fileHandle, dataMem, 0).ConfigureAwait(false);

            // Read compressed block
            await RandomAccess.ReadAsync(_fileHandle, dataMem, DATA_LENGTH).ConfigureAwait(false);
            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4].Span);
            uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8].Span);

            if (compressionIndicator != COMPRESSION_INDICATOR)
                throw new InvalidDataException("Compression indicator not found");

            _inflater.SetInput(data[8..]);

            data = new byte[decompressedLength];
            _inflater.Inflate(data);

            _inflater.Reset();
            return data;
        }

        [Benchmark]
        public async ValueTask<ReadOnlyMemory<byte>> RandomAccessValueTaskAsync()
        {
            // Read uncompressed block
            byte[] data = new byte[DATA_LENGTH];
            Memory<byte> dataMem = new(data);

            await RandomAccess.ReadAsync(_fileHandle, dataMem, 0).ConfigureAwait(false);

            // Read compressed block
            await RandomAccess.ReadAsync(_fileHandle, dataMem, DATA_LENGTH).ConfigureAwait(false);
            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4].Span);
            uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8].Span);

            if (compressionIndicator != COMPRESSION_INDICATOR)
                throw new InvalidDataException("Compression indicator not found");

            _inflater.SetInput(data[8..]);

            data = new byte[decompressedLength];
            _inflater.Inflate(data);

            _inflater.Reset();
            return data;
        }

        [Benchmark]
        public ReadOnlySpan<byte> RandomAccessSync()
        {
            // Read uncompressed block
            byte[] data = new byte[DATA_LENGTH];
            Span<byte> dataSpan = new(data);

            RandomAccess.Read(_fileHandle, dataSpan, 0);

            // Read compressed block
            RandomAccess.Read(_fileHandle, dataSpan, DATA_LENGTH);
            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataSpan[0..4]);
            uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataSpan[4..8]);

            if (compressionIndicator != COMPRESSION_INDICATOR)
                throw new InvalidDataException("Compression indicator not found");

            _inflater.SetInput(data[8..]);

            data = new byte[decompressedLength];
            _inflater.Inflate(data);

            _inflater.Reset();
            return data;
        }

        [Benchmark]
        public async Task<ReadOnlyMemory<byte>> StreamAsync()
        {
            byte[] data = new byte[DATA_LENGTH];
            Memory<byte> dataMem = new(data);

            // Read compressed block
            _fileStream.Seek(DATA_LENGTH, SeekOrigin.Begin);
            await _fileStream.ReadAsync(dataMem[0..8]).ConfigureAwait(false);

            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4].Span);
            uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8].Span);

            if (compressionIndicator != COMPRESSION_INDICATOR)
                throw new InvalidDataException("Compression indicator not found");

            using InflaterInputStream inflaterStream = new(_fileStream)
            {
                IsStreamOwner = false
            };
            await inflaterStream.ReadAsync(dataMem).ConfigureAwait(false);

            // Read uncompressed block
            _fileStream.Seek(0, SeekOrigin.Begin);
            await _fileStream.ReadAsync(dataMem).ConfigureAwait(false);

            return data;
        }

        [Benchmark]
        public async Task<ReadOnlyMemory<byte>> ZlibStreamAsync()
        {
            byte[] data = new byte[DATA_LENGTH];
            Memory<byte> dataMem = new(data);

            // Read compressed block
            _fileStream.Seek(DATA_LENGTH, SeekOrigin.Begin);
            await _fileStream.ReadAsync(dataMem[0..8]).ConfigureAwait(false);

            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4].Span);
            uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8].Span);

            if (compressionIndicator != COMPRESSION_INDICATOR)
                throw new InvalidDataException("Compression indicator not found");

            using ZLibStream inflaterStream = new(_fileStream, CompressionMode.Decompress, true);
            await inflaterStream.ReadAsync(dataMem).ConfigureAwait(false);

            // Read uncompressed block
            _fileStream.Seek(0, SeekOrigin.Begin);
            await _fileStream.ReadAsync(dataMem).ConfigureAwait(false);

            return data;
        }

        [Benchmark]
        public ReadOnlySpan<byte> StreamSync()
        {
            byte[] data = new byte[DATA_LENGTH];
            Span<byte> dataMem = new(data);

            // Read compressed block
            _fileStream.Seek(DATA_LENGTH, SeekOrigin.Begin);
            _fileStream.Read(dataMem[0..8]);

            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4]);
            uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8]);

            if (compressionIndicator != COMPRESSION_INDICATOR)
                throw new InvalidDataException("Compression indicator not found");

            using InflaterInputStream inflaterStream = new(_fileStream)
            {
                IsStreamOwner = false
            };
            inflaterStream.Read(dataMem);

            // Read uncompressed block
            _fileStream.Seek(0, SeekOrigin.Begin);
            _fileStream.Read(dataMem);

            return data;
        }

        [Benchmark]
        public ReadOnlySpan<byte> ZlibStreamSync()
        {
            byte[] data = new byte[DATA_LENGTH];
            Span<byte> dataMem = new(data);

            // Read compressed block
            _fileStream.Seek(DATA_LENGTH, SeekOrigin.Begin);
            _fileStream.Read(dataMem[0..8]);

            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4]);
            uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8]);

            if (compressionIndicator != COMPRESSION_INDICATOR)
                throw new InvalidDataException("Compression indicator not found");

            using ZLibStream inflaterStream = new(_fileStream, CompressionMode.Decompress, true);
            inflaterStream.Read(dataMem);

            // Read uncompressed block
            _fileStream.Seek(0, SeekOrigin.Begin);
            _fileStream.Read(dataMem);

            return data;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _fileHandle.Dispose();
            _fileStream.Dispose();
            File.Delete(_dataFile);
        }
    }
}
