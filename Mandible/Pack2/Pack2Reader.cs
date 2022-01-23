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
    /// Gets the indicator placed in front of an asset data block to indicate that it has been compressed.
    /// </summary>
    protected const uint AssetCompressionIndicator = 0xA1B2C3D4;

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
    public virtual async ValueTask<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(CancellationToken ct = default)
    {
        Pack2Header header = await ReadHeaderAsync(ct).ConfigureAwait(false);
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
    public virtual async ValueTask<int> GetAssetLengthAsync(Asset2Header header, CancellationToken ct = default)
    {
        if (header.ZipStatus is Asset2ZipDefinition.Unzipped or Asset2ZipDefinition.UnzippedAlternate)
            return (int)header.DataSize;

        using IMemoryOwner<byte> bufferOwner = _memoryPool.Rent(8);
        Memory<byte> buffer = bufferOwner.Memory[..8];

        await _dataReader.ReadAsync(buffer, (long)header.DataOffset, ct).ConfigureAwait(false);
        return (int)BinaryPrimitives.ReadUInt32BigEndian(buffer[4..8].Span);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the output buffer was not long enough.</exception>
    public virtual async Task<int> ReadAssetDataAsync
    (
        Asset2Header header,
        Memory<byte> outputBuffer,
        CancellationToken ct = default
    )
    {
        if (outputBuffer.Length < (int)header.DataSize)
        {
            throw new ArgumentOutOfRangeException
            (
                nameof(outputBuffer),
                outputBuffer.Length,
                $"The output buffer must be at least {header.DataSize} bytes in length."
            );
        }

        await _dataReader.ReadAsync
        (
            outputBuffer[..(int)header.DataSize],
            (long)header.DataOffset,
            ct
        ).ConfigureAwait(false);

        if (header.ZipStatus is Asset2ZipDefinition.Zipped or Asset2ZipDefinition.ZippedAlternate)
        {
            using IMemoryOwner<byte> tempBuffer = _memoryPool.Rent((int)header.DataSize);
            outputBuffer[..(int)header.DataSize].CopyTo(tempBuffer.Memory);

            return (int)await UnzipAssetData(tempBuffer.Memory[..(int)header.DataSize], outputBuffer, ct).ConfigureAwait(false);
        }

        return (int)header.DataSize;
    }

    /// <summary>
    /// Unzips an asset.
    /// </summary>
    /// <param name="data">The compressed asset data.</param>
    /// <param name="outputBuffer">The output buffer.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The length of the decompressed data.</returns>
    /// <exception cref="InvalidDataException">Thrown if the compressed data was not in the expected format.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the output buffer was not long enough.</exception>
    protected virtual async Task<uint> UnzipAssetData
    (
        ReadOnlyMemory<byte> data,
        Memory<byte> outputBuffer,
        CancellationToken ct
    )
    {
        // Read the compression information
        uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(data[..4].Span);
        uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(data[4..8].Span);

        if (compressionIndicator != AssetCompressionIndicator)
            throw new InvalidDataException("The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data.");

        if (outputBuffer.Length < decompressedLength)
        {
            throw new ArgumentOutOfRangeException
            (
                nameof(outputBuffer),
                outputBuffer.Length,
                $"The output buffer must be at least {decompressedLength} bytes in length."
            );
        }

        await Task.Run
        (
            () =>
            {
                _inflater.Inflate(data[8..].Span, outputBuffer.Span);
                _inflater.Reset();
            },
            CancellationToken.None // TODO: Does this solve cancelling issues?
        ).ConfigureAwait(false);

        return decompressedLength;
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

            if (compressionIndicator != AssetCompressionIndicator)
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
