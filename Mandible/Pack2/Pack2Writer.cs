using Mandible.Abstractions.Pack2;
using Mandible.Abstractions.Services;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ZlibNGSharpMinimal.Deflate;

namespace Mandible.Pack2;

/// <inheritdoc cref="IPack2Writer" />
public sealed class Pack2Writer : IPack2Writer, IAsyncDisposable
{
    private const int DATA_START_OFFSET = 0x200;
    private const uint COMPRESSED_ASSET_MAGIC = 0xA1B2C3D4;

    private readonly IDataWriterService _writer;
    private readonly List<Asset2Header> _assetMap;
    private readonly ZngDeflater _deflater;
    private readonly IAssetHashProvider _hashProvider;

    private long _currentOffset;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Pack2Writer"/>
    /// instance has been disposed.
    /// </summary>
    public bool IsClosed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pack2Writer"/> class.
    /// </summary>
    /// <param name="writer">The data writer to use.</param>
    /// <param name="crcProvider">The CRC provider, used to calculated hashes of any written asset data.</param>
    public Pack2Writer(IDataWriterService writer, IAssetHashProvider? crcProvider = null)
    {
        _writer = writer;
        _assetMap = new List<Asset2Header>();
        _currentOffset = DATA_START_OFFSET;
        _deflater = new ZngDeflater(CompressionLevel.BestCompression);
        _hashProvider = crcProvider ?? DefaultHashProvider.Shared;
    }

    /// <inheritdoc />
    public async ValueTask WriteAssetAsync
    (
        ulong assetNameHash,
        ReadOnlyMemory<byte> assetData,
        Asset2ZipDefinition zip,
        uint? dataHashOverride = null,
        bool raw = false,
        CancellationToken ct = default
    )
    {
        byte[] compressed = Array.Empty<byte>();
        bool compress = !raw && zip is Asset2ZipDefinition.Zipped or Asset2ZipDefinition.ZippedAlternate;

        if (compress)
        {
            int offset = 0;

            compressed = ArrayPool<byte>.Shared.Rent(assetData.Length + sizeof(uint) + sizeof(uint));

            BinaryPrimitives.WriteUInt32BigEndian(compressed.AsSpan(offset), COMPRESSED_ASSET_MAGIC);
            offset += sizeof(uint);

            BinaryPrimitives.WriteUInt32BigEndian(compressed.AsSpan(offset), (uint)assetData.Length);
            offset += sizeof(uint);

            ulong deflatedLength = _deflater.Deflate
            (
                assetData.Span,
                compressed.AsSpan(offset)
            );
            _deflater.Reset();

            assetData = compressed.AsMemory(0, offset + (int)deflatedLength);
        }

        Asset2Header header = new
        (
            assetNameHash,
            (ulong)_currentOffset,
            (ulong)assetData.Length,
            zip,
            0
        );

        uint crc = dataHashOverride ?? _hashProvider.CalculateDataHash(header, assetData.Span);
        header = header with { DataHash = crc };

        _assetMap.Add(header);
        await _writer.WriteAsync(assetData, _currentOffset, ct).ConfigureAwait(false);

        _currentOffset += assetData.Length;
        IncrementOffsetToNextBoundary();

        if (compress)
            ArrayPool<byte>.Shared.Return(compressed);
    }

    /// <inheritdoc />
    public async ValueTask CloseAsync(CancellationToken ct = default)
    {
        if (IsClosed)
            return;

        IncrementOffsetToNextBoundary();
        ulong packLength = (ulong)_currentOffset + (ulong)(Asset2Header.Size * _assetMap.Count);
        Pack2Header header = new((uint)_assetMap.Count, packLength, (ulong)_currentOffset, new byte[128]);

        byte[] assetBuffer = new byte[Asset2Header.Size];
        foreach (Asset2Header assetHeader in _assetMap)
        {
            assetHeader.Serialize(assetBuffer);
            await _writer.WriteAsync(assetBuffer, _currentOffset, ct).ConfigureAwait(false);
            _currentOffset += Asset2Header.Size;
        }

        byte[] packBuffer = new byte[Pack2Header.Size];
        header.Serialize(packBuffer);
        await _writer.WriteAsync(packBuffer, 0, ct).ConfigureAwait(false);

        IsClosed = true;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        _deflater.Dispose();
    }

    /// <summary>
    /// Increments the <see cref="_currentOffset"/> to the nearest 256-byte boundary.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncrementOffsetToNextBoundary()
    {
        _currentOffset += 0x100;
        _currentOffset = (long)((ulong)_currentOffset & 0xFFFFFFFFFFFFFF00);
    }

    private class DefaultHashProvider : IAssetHashProvider
    {
        public static readonly DefaultHashProvider Shared = new();

        public uint CalculateDataHash(Asset2Header header, ReadOnlySpan<byte> data)
            => 0;
    }
}
