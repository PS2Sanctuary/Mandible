using Mandible.Pack2;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Abstractions.Pack2;

/// <summary>
/// Represents an interface for writing data to a pack2 file.
/// </summary>
public interface IPack2Writer
{
    /// <summary>
    /// Writes an asset to the pack.
    /// </summary>
    /// <param name="assetNameHash">The CRC-64 hash of the asset's name.</param>
    /// <param name="assetData">The asset data.</param>
    /// <param name="zip">Indicates whether the asset data should be compressed.</param>
    /// <param name="dataHashOverride">Overrides the data hash of the written asset.</param>
    /// <param name="raw">Indicates whether the data may be transformed in any way (i.e. compressed).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the potentially asynchronous operation.</returns>
    ValueTask WriteAssetAsync
    (
        ulong assetNameHash,
        ReadOnlyMemory<byte> assetData,
        Asset2ZipDefinition zip,
        uint? dataHashOverride = null,
        bool raw = false,
        CancellationToken ct = default
    );

    /// <summary>
    /// Closes the writer.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the potentially asynchronous operation.</returns>
    ValueTask CloseAsync(CancellationToken ct = default);
}
