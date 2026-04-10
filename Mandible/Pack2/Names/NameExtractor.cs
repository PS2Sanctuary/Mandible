using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Common;
using Mandible.Services;
using Mandible.Util;
using MemoryReaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    /// <param name="existingNamelist">
    /// An existing namelist - both to use as a base for the new namelist, and to speed up extraction times by allowing
    /// certain assets to be ignored based on their filename, instead of needing to read and check the data.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The newly-built extracted namelist.</returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown if the <paramref name="packDirectoryPath"/> does not exist.
    /// </exception>
    public static async Task<Namelist> ExtractAsync
    (
        string packDirectoryPath,
        Namelist? existingNamelist,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(packDirectoryPath))
            throw new DirectoryNotFoundException("The pack directory path does not exist: " + packDirectoryPath);

        return await ExtractAsync
        (
            Directory.EnumerateFiles(packDirectoryPath, "*.pack2", SearchOption.AllDirectories),
            existingNamelist,
            ct
        );
    }

    /// <summary>
    /// Extracts names from pack2 files.
    /// </summary>
    /// <param name="pack2FilePaths">A list of pack2 file paths.</param>
    /// <param name="existingNamelist">
    /// An existing namelist - both to use as a base for the new namelist, and to speed up extraction times by allowing
    /// certain assets to be ignored based on their filename, instead of needing to read and check the data.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The newly-built extracted namelist.</returns>
    public static async Task<Namelist> ExtractAsync
    (
        IEnumerable<string> pack2FilePaths,
        Namelist? existingNamelist,
        CancellationToken ct = default
    )
    {
        Namelist nl = existingNamelist is null
            ? new Namelist()
            : new Namelist(existingNamelist.Map);

        foreach (string packPath in pack2FilePaths)
        {
            using RandomAccessDataReaderService dataReader = new(packPath);
            using Pack2Reader reader = new(dataReader);

            await ExtractFromAssetsAsync(reader, nl, ct);
        }

        return nl;
    }

    private static async Task ExtractFromAssetsAsync(Pack2Reader reader, Namelist namelist, CancellationToken ct)
    {
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct);

        foreach (Asset2Header asset in assetHeaders)
        {
            ct.ThrowIfCancellationRequested();

            FileType inferredType = FileType.Unknown;
            if (namelist.TryGet(asset.NameHash, out string? assetName))
            {
                string extension = Path.GetExtension(assetName);
                inferredType = FileIdentifiers.InferFileType(extension);
            }

            using MemoryOwner<byte> buffer = await reader.ReadAssetDataAsync(asset, false, ct);

            if (asset.NameHash == NamelistFileNameHash)
            {
                ProcessNamelistFile(buffer.Span, namelist);
            }
            else if (AssetNameScraper.IsScrapeableAsset(inferredType))
            {
                IReadOnlyList<string> names = AssetNameScraper.ScrapeFromAssetData(buffer.Span);
                namelist.Append(names, ct);
            }

            if (asset.NameHash == ObjectTerrainDataNameHash)
                await GuessWorldNamesAsync(buffer.Memory, namelist, ct);
        }
    }

    private static void ProcessNamelistFile(ReadOnlySpan<byte> data, Namelist namelist)
    {
        List<string> readNames = [];
        SpanReader<byte> textReader = new(data);

        while (textReader.TryReadToAny(out ReadOnlySpan<byte> name, "\r\n"u8, advancePastDelimiter: true))
        {
            string nameStr = Encoding.UTF8.GetString(name);
            readNames.Add(nameStr);
            AssetNameScraper.AddGlobalFileNamePatterns(nameStr, readNames);

            // If the file has \r then we'll only have read up to this
            textReader.IsNext((byte)'\n', advancePast: true);
        }

        namelist.Append(readNames);
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

            // World tile info files (these list the DDS image tiles used to show the world map)
            for (int i = 0; i < 4; i++)
                names.Add($"{name}_TileInfo_LOD{i}.txt");

            // World chunk files (.cnk0 -> .cnk3)
            for (int i = 0; i <= 3; i++)
            {
                int increment = i switch
                {
                    0 or 1 => 4,
                    2 or 3 => 8,
                    _ => throw new InvalidOperationException()
                };

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
