using Mandible.Abstractions.Pack2;
using Mandible.Abstractions.Services;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
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
        _deflater = new ZngDeflater();
    }

    /// <inheritdoc />
    public async ValueTask WriteAssetAsync
    (
        ulong assetNameHash,
        ReadOnlyMemory<byte> assetData,
        Asset2ZipDefinition zip,
        uint crcDataHash = 0,
        CancellationToken ct = default
    )
    {
        byte[] compressed = Array.Empty<byte>();
        bool compress = zip is Asset2ZipDefinition.Zipped or Asset2ZipDefinition.ZippedAlternate;

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

            assetData = compressed.AsMemory(0, (int)deflatedLength);
        }

        _assetMap.Add(new Asset2Header
        (
            assetNameHash,
            (ulong)_currentOffset,
            (ulong)assetData.Length,
            zip,
            crcDataHash
        ));

        await _writer.WriteAsync(assetData, _currentOffset, ct).ConfigureAwait(false);
        _currentOffset += assetData.Length;

        if (compress)
            ArrayPool<byte>.Shared.Return(compressed);
    }

    /// <inheritdoc />
    public async ValueTask CloseAsync(CancellationToken ct = default)
    {
        if (IsClosed)
            return;

        // Increment to the nearest 256-byte boundary
        _currentOffset += 0x100;
        _currentOffset = (int)((uint)_currentOffset & 0xFFFFFF00);

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
}
