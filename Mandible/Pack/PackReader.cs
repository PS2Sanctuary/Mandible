using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack;
using Mandible.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack;

// TODO: Set public once finalised
/// <inheritdoc cref="IPackReader" />
internal class PackReader : IPackReader
{
    protected readonly IDataReaderService _dataReader;

    public PackReader(IDataReaderService dataReader)
    {
        _dataReader = dataReader;
    }

    public Task<IReadOnlyList<PackChunkHeader>> ReadHeadersAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task<MemoryOwner<byte>> ReadAssetDataAsync(AssetHeader header, CancellationToken ct = default) => throw new NotImplementedException();
}
