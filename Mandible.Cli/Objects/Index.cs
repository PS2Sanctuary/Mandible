using Mandible.Pack2;
using Mandible.Pack2.Names;
using System;
using System.Collections.Generic;
using static Mandible.Cli.Objects.Index2;

namespace Mandible.Cli.Objects;

/// <summary>
/// Initializes a new instance of the <see cref="Index2"/> record.
/// </summary>
/// <param name="GenerationTime">The time that the index was generated.</param>
/// <param name="Packs">The pack indexes.</param>
public record Index2
(
    DateTimeOffset GenerationTime,
    IReadOnlyList<Index2Pack> Packs
)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Index2Pack"/> record.
    /// </summary>
    /// <param name="Path">The path of the pack file that was index.</param>
    /// <param name="PackHeader">The header of the pack file.</param>
    /// <param name="Assets">The assets in the pack file.</param>
    public record Index2Pack
    (
        string Path,
        Pack2Header PackHeader,
        IReadOnlyList<Index2Asset> Assets
    );

    /// <summary>
    /// Initializes a new instance of the <see cref="Index2Asset"/> record.
    /// </summary>
    /// <param name="Name">The name of the asset.</param>
    /// <param name="DataHash">The CRC-32 hash of the asset's data.</param>
    public record Index2Asset
    (
        string Name,
        uint DataHash
    )
    {
        public static Index2Asset FromAsset2Header(Asset2Header header, Namelist namelist)
        {
            namelist.TryGet(header.NameHash, out string? name);
            return new Index2Asset(name ?? header.NameHash.ToString(), header.DataHash);
        }
    }
}
