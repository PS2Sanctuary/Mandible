using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Abstractions.Services;
using Mandible.Exceptions;
using Mandible.Util.Zlib;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2;

/// <inheritdoc cref="IPack2Reader" />
public class Pack2Reader : IPack2Reader, IDisposable
{
    // Average file length is ~0.12 MiB
    // Max file length at time of writing was 93 MiB
    // Max array length = 2 MiB
    private const int SMALL_ARRAY_POOL_CUTOFF = 2097152;
    private static readonly ArrayPool<byte> SmallArrayPool = ArrayPool<byte>.Create(SMALL_ARRAY_POOL_CUTOFF, 50);
    // Max array length = 10 MiB
    private static readonly ArrayPool<byte> LargeArrayPool = ArrayPool<byte>.Create(10485760, 10);

    /// <summary>
    /// Gets the <see cref="IDataReaderService"/> that this <see cref="Pack2Reader"/> was initialized with.
    /// </summary>
    protected readonly IDataReaderService _dataReader;

    /// <summary>
    /// Gets the <see cref="ZlibInflater"/> that this <see cref="Pack2Reader"/> was initialized with.
    /// </summary>
    protected readonly ZlibInflater _inflater;

    /// <summary>
    /// Gets a value indicating whether the underlying data source has been validated.
    /// </summary>
    protected bool IsValid { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Pack2Reader"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pack2Reader"/> class.
    /// </summary>
    /// <param name="dataReader">A <see cref="IDataReaderService"/> configured to read from a pack2 file.</param>
    public Pack2Reader(IDataReaderService dataReader)
    {
        _dataReader = dataReader;
        _inflater = new ZlibInflater(zlibHeaderPresent: true);
    }

    /// <inheritdoc />
    public virtual async ValueTask<Pack2Header> ReadHeaderAsync(CancellationToken ct = default)
    {
        await ValidateAsync(ct);
        using MemoryOwner<byte> data = MemoryOwner<byte>.Allocate(Pack2Header.Size);

        await _dataReader.ReadAsync(data.Memory, 0, ct);

        return Pack2Header.Deserialize(data.Span);
    }

    /// <inheritdoc />
    public virtual async ValueTask<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(CancellationToken ct = default)
    {
        await ValidateAsync(ct);

        Pack2Header header = await ReadHeaderAsync(ct);
        List<Asset2Header> assetHeaders = [];

        int bufferSize = (int)header.AssetCount * Asset2Header.Size;
        using MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate(bufferSize);

        await _dataReader.ReadAsync(buffer.Memory, (long)header.AssetMapOffset, ct);

        for (uint i = 0; i < header.AssetCount; i++)
        {
            int baseOffset = (int)i * Asset2Header.Size;
            Memory<byte> assetHeaderData = buffer.Memory.Slice(baseOffset, Asset2Header.Size);

            Asset2Header assetHeader = Asset2Header.Deserialize(assetHeaderData.Span);
            assetHeaders.Add(assetHeader);
        }

        return assetHeaders;
    }

    /// <inheritdoc />
    public virtual async ValueTask<int> GetAssetLengthAsync(Asset2Header header, CancellationToken ct = default)
    {
        await ValidateAsync(ct);

        if (header.ZipStatus is Asset2ZipDefinition.Unzipped or Asset2ZipDefinition.UnzippedAlternate)
            return (int)header.DataSize;

        using MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate(8);

        await _dataReader.ReadAsync(buffer.Memory, (long)header.DataOffset, ct);
        return (int)BinaryPrimitives.ReadUInt32BigEndian(buffer.Span[4..8]);
    }

    /// <inheritdoc />
    public virtual async ValueTask<MemoryOwner<byte>> ReadAssetDataAsync
    (
        Asset2Header header,
        bool raw = false,
        CancellationToken ct = default
    )
    {
        await ValidateAsync(ct);

        ArrayPool<byte> pool = header.DataSize > SMALL_ARRAY_POOL_CUTOFF
            ? LargeArrayPool
            : SmallArrayPool;
        MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate((int)header.DataSize, pool);

        await _dataReader.ReadAsync
        (
            buffer.Memory,
            (long)header.DataOffset,
            ct
        );

        if (raw || header.ZipStatus is Asset2ZipDefinition.Unzipped or Asset2ZipDefinition.UnzippedAlternate)
            return buffer;

        int decompressedLength = AssetZipHelper.GetDecompressedLength(buffer.Span);
        MemoryOwner<byte> unzippedBuffer = MemoryOwner<byte>.Allocate(decompressedLength, pool);
        AssetZipHelper.UnzipAssetData(buffer.Span, unzippedBuffer.Span, _inflater);

        buffer.Dispose();
        return unzippedBuffer;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the output buffer was not long enough.</exception>
    public virtual async ValueTask<int> ReadRawAssetDataAsync
    (
        Asset2Header header,
        Memory<byte> outputBuffer,
        CancellationToken ct = default
    )
    {
        await ValidateAsync(ct);
        ArgumentOutOfRangeException.ThrowIfLessThan(outputBuffer.Length, (int)header.DataSize, nameof(outputBuffer));

        return await _dataReader.ReadAsync
        (
            outputBuffer[..(int)header.DataSize],
            (long)header.DataOffset,
            ct
        );
    }

    /// <inheritdoc />
    /// <exception cref="ObjectDisposedException">Thrown if this reader has been disposed.</exception>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if the underlying data source is too short to contain pack2 data.
    /// </exception>
    /// <exception cref="UnrecognisedMagicException">
    /// Thrown if the underlying data source does not appear to represent pack2 data.
    /// </exception>
    /// <exception cref="UnsupportedVersionException">
    /// Thrown if the underlying pack2 data is of an unsupported version.
    /// </exception>
    public virtual async ValueTask ValidateAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (IsValid)
            return;

        // Ensure that there is space for the header
        using MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate(Pack2Header.Size);
        int amountRead = await _dataReader.ReadAsync(buffer.Memory, 0, ct);
        if (amountRead != Pack2Header.Size)
            throw new InvalidBufferSizeException(Pack2Header.Size, amountRead);

        // This checks the magic for us
        Pack2Header header = Pack2Header.Deserialize(buffer.Span);

        if (header.Version is not 1)
            throw new UnsupportedVersionException(1, header.Version);

        // Ensure that the pack is long enough to contain the asset map denoted by the header
        long upperBoundLength = (long)header.AssetMapOffset + Asset2Header.Size * header.AssetCount;
        if (_dataReader.GetLength() < upperBoundLength)
            throw new InvalidBufferSizeException((int)upperBoundLength, buffer.Length);

        IsValid = true;
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
    /// <param name="disposeManaged">A value indicating whether to dispose of managed resources.</param>
    protected virtual void Dispose(bool disposeManaged)
    {
        if (IsDisposed)
            return;

        if (disposeManaged)
            _inflater.Dispose();

        IsDisposed = true;
    }
}
