using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Pack2;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Abstractions.Pack2;

/// <summary>
/// Represents an interface for reading data from a pack2 file.
/// </summary>
public interface IPack2Reader
{
    /// <summary>
    /// Reads the pack header.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A pack header.</returns>
    ValueTask<Pack2Header> ReadHeaderAsync(CancellationToken ct = default);

    /// <summary>
    /// Reads the asset headers.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A list of asset headers.</returns>
    ValueTask<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the length of an asset in bytes. If the asset is compressed, the uncompressed length is retrieved.
    /// </summary>
    /// <param name="header">The asset to retrieve the length of.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The size in bytes of the asset.</returns>
    ValueTask<int> GetAssetLengthAsync(Asset2Header header, CancellationToken ct = default);

    /// <summary>
    /// Reads an asset from the pack. The asset is decompressed if required.
    /// </summary>
    /// <param name="header">The asset to retrieve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A buffer containing the asset data..</returns>
    Task<MemoryOwner<byte>> ReadAssetDataAsync
    (
        Asset2Header header,
        CancellationToken ct = default
    );
}
