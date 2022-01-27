using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack;
using Mandible.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack;

/// <inheritdoc cref="IPackReader" />
public class PackReader : IPackReader
{
    protected readonly IDataReaderService _dataReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackReader"/> class.
    /// </summary>
    /// <param name="dataReader">A <see cref="IDataReaderService"/> configured to read from a pack file.</param>
    public PackReader(IDataReaderService dataReader)
    {
        _dataReader = dataReader;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AssetHeader>> ReadAssetHeadersAsync(CancellationToken ct = default)
    {
        List<AssetHeader> assetHeaders = new();
        long packOffset = 0;

        long dataLength = _dataReader.GetLength();
        int bufferSize = dataLength < 4096
            ? (int)dataLength
            : 4096;

        using MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate(bufferSize);

        do
        {
            await _dataReader.ReadAsync(buffer.Memory, packOffset, ct).ConfigureAwait(false);
            PackChunkHeader header = PackChunkHeader.Deserialize(buffer.Span);

            int bufferOffset = PackChunkHeader.Size;
            for (int i = 0; i < header.AssetCount; i++)
            {
                if (!AssetHeader.TryDeserialize(buffer.Span[bufferOffset..], out AssetHeader? assetHeader))
                {
                    packOffset += bufferOffset;
                    bufferOffset = 0;
                    await _dataReader.ReadAsync(buffer.Memory, packOffset, ct).ConfigureAwait(false);

                    i--;
                    continue;
                }

                bufferOffset += assetHeader.GetSize();
                assetHeaders.Add(assetHeader);
            }

            packOffset = header.NextChunkOffset;
        } while (packOffset < dataLength && packOffset > 0);

        return assetHeaders;
    }

    /// <inheritdoc />
    public Task<MemoryOwner<byte>> ReadAssetDataAsync(AssetHeader header, CancellationToken ct = default) => throw new NotImplementedException();
}
