using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Abstractions.Services;
using Mandible.Util.Zlib;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2;

/// <inheritdoc cref="IPack2Writer" />
public sealed class Pack2Writer : IPack2Writer, IAsyncDisposable
{
    private const int DATA_START_OFFSET = 0x200;

    /// <summary>
    /// Gets the magic data used to indicate that an asset has been compressed.
    /// </summary>
    public const uint COMPRESSED_ASSET_MAGIC = 0xA1B2C3D4;

    private readonly IDataWriterService _writer;
    private readonly List<Asset2Header> _assetMap;
    private readonly ZlibDeflator _deflator;

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
    public Pack2Writer(IDataWriterService writer)
    {
        _writer = writer;
        _assetMap = new List<Asset2Header>();
        _currentOffset = DATA_START_OFFSET;
        _deflator = new ZlibDeflator(ZlibCompressionLevel.BestCompression, true);
    }

    /// <inheritdoc />
    /// <remarks>
    /// If specifying the asset data should be zipped, this method assumes that the compressed data will not be larger
    /// that the uncompressed data length.
    /// </remarks>
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
        MemoryOwner<byte>? compressed = null;
        bool compress = !raw
            && zip is Asset2ZipDefinition.Zipped or Asset2ZipDefinition.ZippedAlternate
            && assetData.Length > 0;

        if (compress)
        {
            int offset = 0;

            // Add space for the compression indicator, uncompressed length and zlib asset header
            compressed = MemoryOwner<byte>.Allocate(assetData.Length + sizeof(uint) * 2 + ZlibConstants.HeaderLength);

            BinaryPrimitives.WriteUInt32BigEndian(compressed.Span[offset..], COMPRESSED_ASSET_MAGIC);
            offset += sizeof(uint);

            BinaryPrimitives.WriteUInt32BigEndian(compressed.Span[offset..], (uint)assetData.Length);
            offset += sizeof(uint);

            int deflatedLength = _deflator.Deflate
            (
                assetData.Span,
                compressed.Span[offset..]
            );
            _deflator.Reset();

            assetData = compressed.Memory[..(offset + deflatedLength)];
        }

        Asset2Header header = new
        (
            assetNameHash,
            (ulong)_currentOffset,
            (ulong)assetData.Length,
            zip,
            dataHashOverride ?? AssetHashCrc32.CalculateDataHash(assetNameHash, assetData.Span)
        );

        _assetMap.Add(header);
        await _writer.WriteAsync(assetData, _currentOffset, ct).ConfigureAwait(false);

        _currentOffset += assetData.Length;
        IncrementOffsetToNextBoundary();

        compressed?.Dispose();
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
        _deflator.Dispose();
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
}
