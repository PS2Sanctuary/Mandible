using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Pack;
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
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A list of chunk headers.</returns>
    Task<IReadOnlyList<AssetHeader>> ReadAssetHeadersAsync(CancellationToken ct = default);

    /// <summary>
    /// Reads the asset data for a given header.
    /// </summary>
    /// <param name="header">The asset to retrieve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>Asset data.</returns>
    Task<MemoryOwner<byte>> ReadAssetDataAsync(AssetHeader header, CancellationToken ct = default);
}
