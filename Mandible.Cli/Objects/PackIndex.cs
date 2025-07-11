﻿using Mandible.Pack;
using Mandible.Pack2;
using Mandible.Pack2.Names;
using System.Collections.Generic;
using static Mandible.Cli.Objects.PackIndex;

namespace Mandible.Cli.Objects;

/// <summary>
/// Initializes a new instance of the <see cref="PackIndex"/> record.
/// </summary>
/// <param name="Path">The path to the pack that this <see cref="PackIndex"/> was generated from.</param>
/// <param name="AssetCount">The number of assets in the pack.</param>
/// <param name="Length">The length of the pack in bytes.</param>
/// <param name="Assets">The pack assets.</param>
public record PackIndex
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
        ulong? NameHash,
        uint DataHash,
        Asset2ZipDefinition? ZipStatus
    )
    {
        public static IndexAsset FromAssetHeader(AssetHeader header)
            => new(header.Name, null, header.Checksum, null);

        public static IndexAsset FromAsset2Header(Asset2Header header, Namelist namelist)
        {
            namelist.TryGet(header.NameHash, out string? name);
            return new IndexAsset(name ?? string.Empty, header.NameHash, header.DataHash, header.ZipStatus);
        }
    }
}
