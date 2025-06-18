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
    IReadOnlyList<PackMetadata> Packs
)
{
    public record PackMetadata
    (
        string Name,
        long Hash,
        uint AssetCount,
        ulong PackLength
    )
    {
        public static PackMetadata FromIndex(PackIndex index)
        {
            string path = Path.GetFileName(index.Path);

            long hash = 17;
            unchecked
            {
                hash = (hash * 23) + index.AssetCount.GetHashCode();
                foreach (PackIndex.IndexAsset pia in index.Assets)
                    hash = (hash * 23) + pia.DataHash;
            }

            return new PackMetadata(path, hash, index.AssetCount, index.Length);
        }
    }

    public static IndexMetadata FromIndexList(IEnumerable<PackIndex> indexes)
        => new
        (
            DateTimeOffset.UtcNow,
            indexes.Select(PackMetadata.FromIndex)
                .OrderBy(i => i.Name)
                .ToList()
        );
}
