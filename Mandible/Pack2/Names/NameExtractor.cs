using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Services;
using Mandible.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mandible.Pack2.Names;

/// <summary>
/// Provides functionalities to extract and guess at names from pack2 files.
/// </summary>
public static class NameExtractor
{
    private static readonly ulong NamelistFileNameHash = PackCrc64.Calculate("{NAMELIST}");
    private static readonly ulong ObjectTerrainDataNameHash = PackCrc64.Calculate("ObjectTerrainData.xml");

    /// <summary>
    /// Extracts names from pack2 files.
    /// </summary>
    /// <param name="packDirectoryPath">The path to the directory containing the pack2 files.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The extracted namelist.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="packDirectoryPath"/> does not exist.</exception>
    public static async Task<Namelist> ExtractAsync
    (
        string packDirectoryPath,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(packDirectoryPath))
            throw new DirectoryNotFoundException("The pack directory path does not exist: " + packDirectoryPath);

        Namelist nl = new();
        foreach (string packPath in Directory.EnumerateFiles(packDirectoryPath, "*.pack2", SearchOption.AllDirectories))
        {
            using RandomAccessDataReaderService dataReader = new(packPath);
            using Pack2Reader reader = new(dataReader);

            await ExtractFromEmbeddedNamelistAsync(reader, nl, ct);
            await ExtractFromAssetsAsync(reader, nl, ct);
        }

        return nl;
    }

    private static async Task ExtractFromEmbeddedNamelistAsync(Pack2Reader reader, Namelist namelist, CancellationToken ct)
    {
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct);

        Asset2Header? namelistHeader = assetHeaders.FirstOrDefault(asset => asset.NameHash == NamelistFileNameHash);
        if (namelistHeader is null)
            return;

        using MemoryOwner<byte> buffer = await reader.ReadAssetDataAsync(namelistHeader, false, ct);
        namelist.Append(buffer.Span);
    }

    private static async Task ExtractFromAssetsAsync(Pack2Reader reader, Namelist namelist, CancellationToken ct)
    {
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct);

        // TODO: Can we use an existing namelist to help trim down the number of files we have to search?
        // E.g. if we encounter a unique extension such as apx which we know is not valid to search,
        // then we can skip loading the asset data - faster!
        foreach (Asset2Header asset in assetHeaders)
        {
            ct.ThrowIfCancellationRequested();
            using MemoryOwner<byte> buffer = await reader.ReadAssetDataAsync(asset, false, ct);

            IReadOnlyList<string> names = AssetNameScraper.ScrapeFromAssetData(buffer.Span);
            namelist.Append(names, ct);

            if (asset.NameHash == ObjectTerrainDataNameHash)
                await GuessWorldNamesAsync(buffer.Memory, namelist, ct);
        }
    }

    private static async Task GuessWorldNamesAsync
    (
        ReadOnlyMemory<byte> objectTerrainDataXmlBuffer,
        Namelist namelist,
        CancellationToken ct
    )
    {
        List<string> names = [];

        XmlReaderSettings xmlSettings = new()
        {
            Async = true,
            ConformanceLevel = ConformanceLevel.Fragment // This is required as each definition is a root-level object.
        };
        using XmlReader xml = XmlReader.Create(new MemoryStream(objectTerrainDataXmlBuffer.ToArray()), xmlSettings);

        while (await xml.ReadAsync())
        {
            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            if (xml.NodeType is not XmlNodeType.Element)
                continue;

            if (xml.Name != "ObjectTerrainData")
                continue;

            string? dataName = xml.GetAttribute("DataName");
            if (string.IsNullOrEmpty(dataName))
                continue;

            // Worlds should have a single word in their name
            if (dataName.Contains('_'))
                continue;

            // Worlds should have an associated sky file
            string? skyFileName = xml.GetAttribute("SkyFileName");
            if (string.IsNullOrEmpty(skyFileName))
                continue;

            // Sky file names with multiple underscores are indicative of a non-world
            if (skyFileName.IndexOf('_') != skyFileName.LastIndexOf('_'))
                continue;

            // Worlds should not have a minimap tileset
            string? minimapTileset = xml.GetAttribute("MinimapTileset");
            if (!string.IsNullOrEmpty(minimapTileset))
                continue;

            AddName(dataName);
        }

        // Manually append, the algorithm excludes it because it is referred to as 'VR_Training'.
        AddName("VR");

        // These worlds aren't always present in the terrain data list
        AddName("Desolation");
        AddName("OutfitWars");

        namelist.Append(names, ct);

        return;

        void AddName(string name)
        {
            // World area file
            names.Add(name + "Areas.xml");
            names.Add(name + ".zone");
            names.Add(name + ".vnfo");

            // World tile info files
            for (int i = 0; i < 4; i++)
                names.Add($"{name}_TileInfo_LOD{i}.txt");

            // World chunk files
            for (int i = 0; i < 6; i++)
            {
                int increment = (i - 1) < 0
                    ? 4
                    : 4 * (int)Math.Pow(2, i - 1);

                for (int x = -64; x < 64; x += increment)
                {
                    for (int y = -64; y < 64; y += increment)
                        names.Add($"{name}_{x}_{y}.cnk{i}");
                }
            }

            // World tome files
            for (int x = -4; x < 4; x++)
            {
                for (int y = -4; y < 4; y++)
                    names.Add($"{name}_{x}_{y}.tome");
            }
        }
    }
}
