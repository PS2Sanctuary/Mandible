using Mandible.Pack2;
using System.Buffers;
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
    /// <returns>A pack header.</returns>
    Pack2Header ReadHeader();

    /// <inheritdoc cref="ReadHeader" />
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    Task<Pack2Header> ReadHeaderAsync(CancellationToken ct = default);

    /// <summary>
    /// Reads the asset headers.
    /// </summary>
    /// <param name="header">The header of the pack2 file that this reader is operating on.</param>
    /// <returns>A list of asset headers.</returns>
    IReadOnlyList<Asset2Header> ReadAssetHeaders(Pack2Header header);

    /// <inheritdoc cref="ReadAssetHeaders(Pack2Header)" />
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    Task<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(Pack2Header header, CancellationToken ct = default);

    /// <summary>
    /// Reads the asset data for a given header. The data is unzipped if required.
    /// </summary>
    /// <param name="assetHeader">The asset to retrieve.</param>
    /// <returns>Asset data.</returns>
    IMemoryOwner<byte> ReadAssetData(Asset2Header assetHeader);

    /// <inheritdoc cref="ReadAssetData(Asset2Header)" />
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    Task<IMemoryOwner<byte>> ReadAssetDataAsync(Asset2Header assetHeader, CancellationToken ct = default);
}
