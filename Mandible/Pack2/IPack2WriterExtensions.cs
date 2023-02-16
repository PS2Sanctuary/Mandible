using Mandible.Abstractions.Pack2;
using Mandible.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2;

/// <summary>
/// Contains extension methods for the <see cref="IPack2Writer"/> interface.
/// </summary>
public static class IPack2WriterExtensions
{
    /// <summary>
    /// Writes an asset to the pack.
    /// </summary>
    /// <param name="writer">The <see cref="IPack2Writer"/> to use.</param>
    /// <param name="assetName">The name of the asset.</param>
    /// <param name="assetData">The asset data.</param>
    /// <param name="zip">Indicates whether the asset data should be compressed.</param>
    /// <param name="dataHashOverride">Overrides the data hash of the written asset.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the potentially asynchronous operation.</returns>
    public static ValueTask WriteAssetAsync
    (
        this IPack2Writer writer,
        string assetName,
        ReadOnlyMemory<byte> assetData,
        Asset2ZipDefinition zip,
        uint? dataHashOverride = null,
        CancellationToken ct = default
    )
    {
        ulong nameHash = PackCrc64.Calculate(assetName);
        return writer.WriteAssetAsync(nameHash, assetData, zip, dataHashOverride, ct);
    }

    /// <summary>
    /// Writes an asset to the pack.
    /// </summary>
    /// <param name="writer">The <see cref="IPack2Writer"/> to use.</param>
    /// <param name="asset">The asset header.</param>
    /// <param name="assetData">The asset data.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the potentially asynchronous operation.</returns>
    public static ValueTask WriteAssetAsync
    (
        this IPack2Writer writer,
        Asset2Header asset,
        ReadOnlyMemory<byte> assetData,
        CancellationToken ct = default
    ) => writer.WriteAssetAsync(asset.NameHash, assetData, asset.ZipStatus, asset.DataHash, ct);
}
