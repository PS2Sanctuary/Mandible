using Mandible.Abstractions.Pack;
using Mandible.Abstractions.Services;
using Mandible.Pack2;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack;

/// <inheritdoc />
public class PackReader : IPackReader
{
    protected readonly IDataReaderService _dataReader;
    protected readonly MemoryPool<byte> _memoryPool;

    /// <summary>
    /// Gets a value indicating whether or not this <see cref="Pack2Reader"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; protected set; }

    public PackReader(IDataReaderService dataReader)
    {
        _dataReader = dataReader;

        _memoryPool = MemoryPool<byte>.Shared;
    }

    public virtual IReadOnlyList<PackChunkHeader> ReadHeaders()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<PackChunkHeader>> ReadHeadersAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public IMemoryOwner<byte> ReadAssetData(AssetHeader header) => throw new NotImplementedException();
    public Task<IMemoryOwner<byte>> ReadAssetDataAsync(AssetHeader header, CancellationToken ct = default) => throw new NotImplementedException();
}
