using Mandible.Abstractions.Pack2;
using Mandible.Abstractions.Services;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZlibNGSharpMinimal.Inflate;

namespace Mandible.Pack2;

/// <inheritdoc />
public class Pack2Reader : IPack2Reader, IDisposable
{
    /// <summary>
    /// The indicator placed in front of an asset data block to indicate that it has been compressed.
    /// </summary>
    protected const uint ASSET_COMPRESSION_INDICATOR = 0xA1B2C3D4;

    protected readonly IDataReaderService _dataReader;
    protected readonly ZngInflater _inflater;
    protected readonly MemoryPool<byte> _memoryPool;

    /// <summary>
    /// Gets a value indicating whether or not this <see cref="Pack2Reader"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pack2Reader"/> class.
    /// </summary>
    /// <param name="dataReader">The data reader configured for the pack2 to read from.</param>
    public Pack2Reader(IDataReaderService dataReader)
    {
        _dataReader = dataReader;

        _memoryPool = MemoryPool<byte>.Shared;
        _inflater = new ZngInflater();
    }

    /// <inheritdoc />
    public virtual async ValueTask<Pack2Header> ReadHeaderAsync(CancellationToken ct = default)
    {
        using IMemoryOwner<byte> data = _memoryPool.Rent(Pack2Header.Size);
        Memory<byte> headerBuffer = data.Memory[..Pack2Header.Size];

        await _dataReader.ReadAsync(headerBuffer, 0, ct).ConfigureAwait(false);

        return Pack2Header.Deserialize(headerBuffer.Span);
    }

    /// <inheritdoc />
    public virtual Pack2Header ReadHeader()
    {
        using IMemoryOwner<byte> data = _memoryPool.Rent(Pack2Header.Size);
        Span<byte> headerBuffer = data.Memory[..Pack2Header.Size].Span;

        _dataReader.Read(headerBuffer, 0);

        return Pack2Header.Deserialize(headerBuffer);
    }

    /// <inheritdoc />
    public virtual async ValueTask<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(Pack2Header header, CancellationToken ct = default)
    {
        List<Asset2Header> assetHeaders = new();

        int bufferSize = (int)header.AssetCount * Asset2Header.Size;
        using IMemoryOwner<byte> bufferOwner = _memoryPool.Rent(bufferSize);
        Memory<byte> buffer = bufferOwner.Memory[..bufferSize];

        await _dataReader.ReadAsync(buffer, (long)header.AssetMapOffset, ct).ConfigureAwait(false);

        for (uint i = 0; i < header.AssetCount; i++)
        {
            int baseOffset = (int)i * Asset2Header.Size;
            Memory<byte> assetHeaderData = buffer.Slice(baseOffset, Asset2Header.Size);

            Asset2Header assetHeader = Asset2Header.Deserialize(assetHeaderData.Span);
            assetHeaders.Add(assetHeader);
        }

        return assetHeaders;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<Asset2Header> ReadAssetHeaders(Pack2Header header)
    {
        List<Asset2Header> assetHeaders = new();

        int bufferSize = (int)header.AssetCount * Asset2Header.Size;
        using IMemoryOwner<byte> data = _memoryPool.Rent(bufferSize);
        Span<byte> assetHeadersBuffer = data.Memory[..bufferSize].Span;

        _dataReader.Read(assetHeadersBuffer, (long)header.AssetMapOffset);

        for (uint i = 0; i < header.AssetCount; i++)
        {
            int baseOffset = (int)i * Asset2Header.Size;
            Span<byte> assetHeaderData = assetHeadersBuffer.Slice(baseOffset, Asset2Header.Size);

            Asset2Header assetHeader = Asset2Header.Deserialize(assetHeaderData);
            assetHeaders.Add(assetHeader);
        }

        return assetHeaders;
    }

    /// <inheritdoc />
    public virtual async Task<(IMemoryOwner<byte> Data, int Length)> ReadAssetDataAsync(Asset2Header assetHeader, CancellationToken ct = default)
    {
        int length = (int)assetHeader.DataSize;

        IMemoryOwner<byte> outputOwner = _memoryPool.Rent(length);
        Memory<byte> output = outputOwner.Memory[..length];

        await _dataReader.ReadAsync(output, (long)assetHeader.DataOffset, ct).ConfigureAwait(false);

        if (assetHeader.ZipStatus == Asset2ZipDefinition.Zipped || assetHeader.ZipStatus == Asset2ZipDefinition.ZippedAlternate)
        {
            // Read the compression information
            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(output[..4].Span);
            length = (int)BinaryPrimitives.ReadUInt32BigEndian(output[4..8].Span);

            if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
            {
                throw new InvalidDataException
                (
                    "The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data."
                );
            }

            outputOwner = await Task.Run
            (
                () =>
                {
                    IMemoryOwner<byte> decompData = _memoryPool.Rent(length);

                    _inflater.Inflate(output[8..].Span, decompData.Memory.Span);
                    _inflater.Reset();
                    outputOwner.Dispose();

                    return decompData;
                }
            ).ConfigureAwait(false);
        }

        return (outputOwner, length);
    }

    /// <summary>
    /// Gets a stream to retrieve the unzipped asset data. Note that the asset data is stored and returned in big endian format.
    /// </summary>
    /// <param name="assetHeader">The asset to retrieve.</param>
    /// <returns>A stream of the asset data.</returns>
    public virtual (IMemoryOwner<byte> Data, int Length) ReadAssetData(Asset2Header assetHeader)
    {
        int length = (int)assetHeader.DataSize;

        IMemoryOwner<byte> outputOwner = _memoryPool.Rent(length);
        Span<byte> output = outputOwner.Memory.Span[..length];

        _dataReader.Read(output, (long)assetHeader.DataOffset);

        if (assetHeader.ZipStatus == Asset2ZipDefinition.Zipped || assetHeader.ZipStatus == Asset2ZipDefinition.ZippedAlternate)
        {
            // Read the compression information
            uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(output[..4]);
            length = (int)BinaryPrimitives.ReadUInt32BigEndian(output[4..8]);

            if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
            {
                throw new InvalidDataException
                (
                    "The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data."
                );
            }

            IMemoryOwner<byte> decompData = _memoryPool.Rent(length);

            _inflater.Inflate(output[8..], decompData.Memory.Span);
            _inflater.Reset();
            outputOwner.Dispose();

            outputOwner = decompData;
        }

        return (outputOwner, length);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposeManaged">A value indicating whether or not to dispose of managed resources.</param>
    protected virtual void Dispose(bool disposeManaged)
    {
        if (IsDisposed)
            return;

        if (disposeManaged)
            _inflater.Dispose();

        IsDisposed = true;
    }
}
