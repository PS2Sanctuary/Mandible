using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Mandible.Cli.Objects.IndexMetadata;

namespace Mandible.Cli.Objects;

/// <summary>
/// Initializes a new instance of the <see cref="IndexMetadata"/> record.
/// </summary>
/// <param name="GenerationTime">The time that the index was generated at.</param>
/// <param name="Packs">The packs included in the index at the time of generation.</param>
public record IndexMetadata
(
    DateTimeOffset GenerationTime,
    long TotalAssetCount,
    int TotalUnnamedAssetCount,
    IReadOnlyList<PackMetadata> Packs
)
{
    public record PackMetadata
    (
        string Name,
        long Hash,
        uint AssetCount,
        int UnnamedAssetCount,
        ulong PackLength
    )
    {
        public static PackMetadata FromIndex(PackIndex index)
        {
            string path = Path.GetFileName(index.Path);

            // Hashing the index gives us an easy way to diff changes between indexing
            long hash = 17;
            unchecked
            {
                hash = (hash * 23) + index.AssetCount.GetHashCode();
                foreach (PackIndex.IndexAsset pia in index.Assets)
                    hash = (hash * 23) + pia.DataHash;
            }

            return new PackMetadata(path, hash, index.AssetCount, index.UnnamedAssetCount, index.Length);
        }
    }

    public static IndexMetadata FromIndexList(IEnumerable<PackIndex> indexes)
    {
        PackMetadata[] packMetas = indexes.Select(PackMetadata.FromIndex)
            .OrderBy(i => i.Name)
            .ToArray();

        return new IndexMetadata
        (
            DateTimeOffset.UtcNow,
            packMetas.Sum(x => x.AssetCount),
            packMetas.Sum(x => x.UnnamedAssetCount),
            packMetas
        );
    }
}
