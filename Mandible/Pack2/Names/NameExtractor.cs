using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Services;
using Mandible.Util;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <summary>
    /// Gets the CRC-64 hash of the namelist file name ( {NAMELIST} ).
    /// </summary>
    private const ulong NamelistFileNameHash = 4699449473529019696;

    private static readonly byte[] IllegalNameCharacters = new[] { '!', '"', '#', '$', '%', '&', '*', '+', ',', '/', ':', ';', '=', '>', '?', '@', '\\', '^', '`', '|', '~', '\t', '\r', '\n', ' ' }.Select(c => (byte)c).ToArray();
    private static readonly string[] KnownFileExtensions = { "adr", "agr", "ags", "apb", "apx", "bat", "bin", "cdt", "cnk0", "cnk1", "cnk2", "cnk3", "cnk4", "cnk5", "crc", "crt", "cso", "cur", "dat", "db", "dds", "def", "dir", "dll", "dma", "dme", "dmv", "dsk", "dx11efb", "dx11rsb", "dx11ssb", "eco", "efb", "exe", "fsb", "fxd", "fxo", "gfx", "gnf", "i64", "ini", "jpg", "lst", "lua", "mrn", "pak", "pem", "playerstudio", "png", "prsb", "psd", "pssb", "tga", "thm", "tome", "ttf", "txt", "vnfo", "wav", "xlsx", "xml", "xrsb", "xssb", "zone" };

    /// <summary>
    /// Extracts names from pack2 files. Expect this operation to take multiple minutes
    /// if <paramref name="deepExtract"/> is enabled.
    /// </summary>
    /// <param name="packDirectoryPath">The path to the directory containing the pack2 files.</param>
    /// <param name="deepExtract">
    /// Considerably decreases the speed of the operation in exchange for searching
    /// every single asset for names, rather than just those in the data_x64_0 pack.
    /// Approximately 97% of names will still be extracted with this mode disabled.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The extracted namelist.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown if the <paramref name="packDirectoryPath"/> does not exist.</exception>
    public static async Task<Namelist> ExtractAsync
    (
        string packDirectoryPath,
        bool deepExtract = false,
        CancellationToken ct = default)
    {
        if (!Directory.Exists(packDirectoryPath))
            throw new DirectoryNotFoundException("The pack directory path does not exist: " + packDirectoryPath);

        Namelist nl = new();
        foreach (string packPath in Directory.EnumerateFiles(packDirectoryPath, "*.pack2", SearchOption.AllDirectories))
        {
            using RandomAccessDataReaderService dataReader = new(packPath);
            using Pack2Reader reader = new(dataReader);

            await ExtractFromEmbeddedNamelistAsync(reader, nl, ct).ConfigureAwait(false);

            if (deepExtract)
                await ExtractFromAssetsAsync(reader, nl, ct).ConfigureAwait(false);

            string fileName = Path.GetFileNameWithoutExtension(packPath);
            if (fileName == "data_x64_0" && !deepExtract)
                await ExtractFromAssetsAsync(reader, nl, ct).ConfigureAwait(false);
        }

        return nl;
    }

    private static async Task ExtractFromEmbeddedNamelistAsync(IPack2Reader reader, Namelist namelist, CancellationToken ct)
    {
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        Asset2Header? namelistHeader = null;
        foreach (Asset2Header asset in assetHeaders)
        {
            if (asset.NameHash == NamelistFileNameHash)
            {
                namelistHeader = asset;
                break;
            }
        }

        if (namelistHeader is null)
            return;

        using MemoryOwner<byte> buffer = await reader.ReadAssetDataAsync(namelistHeader, ct).ConfigureAwait(false);
        namelist.Append(buffer.Span);
    }

    private static async Task ExtractFromAssetsAsync(IPack2Reader reader, Namelist namelist, CancellationToken ct)
    {
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        foreach (Asset2Header asset in assetHeaders)
        {
            using MemoryOwner<byte> buffer = await reader.ReadAssetDataAsync(asset, ct).ConfigureAwait(false);

            if (IsBinaryFile(buffer.Span))
                continue;

            List<string> names = ExtractNamesFromTextData(buffer.Memory);
            await namelist.Append(names, ct).ConfigureAwait(false);

            if (asset.NameHash == PackCrc64.Calculate("ObjectTerrainData.xml"))
                await GuessWorldNamesAsync(buffer.Memory, namelist, ct).ConfigureAwait(false);
        }
    }

    private static List<string> ExtractNamesFromTextData(ReadOnlyMemory<byte> data)
    {
        byte[][] fileExtensions = KnownFileExtensions.Select(s => Encoding.ASCII.GetBytes('.' + s)).ToArray();
        List<string> names = new();
        ReadOnlySequence<byte> sequence = new(data);
        SequenceReader<byte> reader = new(sequence);

        while (reader.TryAdvanceTo((byte)'.'))
        {
            for (int i = 0; i < fileExtensions.Length; i++)
            {
                byte[] extension = fileExtensions[i];

                if (!reader.IsNext(extension.AsSpan()[1..]))
                    continue;

                // Find the start of the name
                byte currentChar;
                bool doNotAdvance = false;
                do
                {
                    reader.Rewind(1);
                    reader.TryPeek(out currentChar);

                    if (reader.Consumed == 0)
                        doNotAdvance = true;
                } while (!IllegalNameCharacters.Contains(currentChar) && !doNotAdvance);

                if (!doNotAdvance)
                    reader.Advance(1);

                reader.TryReadTo(out ReadOnlySpan<byte> nameBytes, extension);
                string name = Encoding.ASCII.GetString(nameBytes) + '.' + KnownFileExtensions[i];
                names.Add(name);

                break;
            }
        }

        return names;
    }

    private static async Task GuessWorldNamesAsync(ReadOnlyMemory<byte> objectTerrainDataXmlBuffer, Namelist namelist, CancellationToken ct)
    {
        List<string> names = new();

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

        XmlReaderSettings xmlSettings = new()
        {
            Async = true,
            ConformanceLevel = ConformanceLevel.Fragment // This is required as each definition is a root-level object.
        };
        using XmlReader xml = XmlReader.Create(new MemoryStream(objectTerrainDataXmlBuffer.ToArray()), xmlSettings);

        while (await xml.ReadAsync().ConfigureAwait(false))
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

        // Manually append this, the algorithm excludes it because it is referred to as 'VR_Training'.
        AddName("VR");

        await namelist.Append(names, ct).ConfigureAwait(false);
    }

    private static bool IsBinaryFile(ReadOnlySpan<byte> data)
    {
        for (int i = 0; i < data.Length && i < 8000; i++)
        {
            if (data[i] == 0)
                return true;
        }

        return false;
    }
}
