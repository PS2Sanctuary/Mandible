using Mandible.Pack;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Abstractions.Pack;

/// <summary>
/// Represents an interface for reading data from a pack file.
/// </summary>
public interface IPackReader
{
    /// <summary>
    /// Reads the chunk headers.
    /// </summary>
    /// <returns>A list of chunk headers.</returns>
    IReadOnlyList<PackChunkHeader> ReadHeaders();

    /// <inheritdoc cref="ReadHeaders" />
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    Task<IReadOnlyList<PackChunkHeader>> ReadHeadersAsync(CancellationToken ct = default);

    /// <summary>
    /// Reads the asset data for a given header.
    /// </summary>
    /// <param name="header">The asset to retrieve.</param>
    /// <returns>Asset data.</returns>
    IMemoryOwner<byte> ReadAssetData(AssetHeader header);

    /// <inheritdoc cref="ReadAssetData(AssetHeader)" />
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    Task<IMemoryOwner<byte>> ReadAssetDataAsync(AssetHeader header, CancellationToken ct = default);
}
