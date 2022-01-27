using Mandible.Pack2;
using Mandible.Pack2.Names;
using System.Collections.Generic;
using static Mandible.Cli.Objects.Index;

namespace Mandible.Cli.Objects;

/// <summary>
/// Initializes a new instance of the <see cref="Index"/> record.
/// </summary>
/// <param name="Path">The path to the pack that this <see cref="Index"/> was generated from.</param>
/// <param name="AssetCount">The number of assets in the pack.</param>
/// <param name="Length">The length of the pack in bytes.</param>
/// <param name="Assets">The pack assets.</param>
public record Index
(
    string Path,
    uint AssetCount,
    ulong Length,
    IReadOnlyList<IndexAsset> Assets
)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexAsset"/> record.
    /// </summary>
    /// <param name="Name">The name of the asset.</param>
    /// <param name="DataHash">The CRC-32 hash of the asset's data.</param>
    public record IndexAsset
    (
        string Name,
        uint DataHash
    )
    {
        public static IndexAsset FromAsset2Header(Asset2Header header, Namelist namelist)
        {
            namelist.TryGet(header.NameHash, out string? name);
            return new IndexAsset(name ?? header.NameHash.ToString(), header.DataHash);
        }
    }
}
