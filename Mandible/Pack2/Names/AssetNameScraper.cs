using BinaryPrimitiveHelpers;
using Mandible.Common;
using Mandible.Dma;
using Mandible.Fsb;
using Mandible.Zone;
using MemoryReaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mandible.Pack2.Names;

/// <summary>
/// A utility class for scraping asset names from PACK2 asset data.
/// </summary>
public static class AssetNameScraper
{
    private delegate void DedicatedAssetHandler(ReadOnlySpan<byte> assetData, ICollection<string> namesOutput);

    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> UNSCRAPEABLE_FILE_MAGICS;
    private static readonly IReadOnlyList<(ReadOnlyMemory<byte> Magic, DedicatedAssetHandler Handler)> DEDICATED_ASSET_HANDLERS;
    private static readonly IReadOnlyList<byte[]> KNOWN_FILE_EXTENSIONS;

    static AssetNameScraper()
    {
        UNSCRAPEABLE_FILE_MAGICS =
        [
            FileIdentifiers.Magics[FileType.CollisionData],
            FileIdentifiers.Magics[FileType.Gfx],
            FileIdentifiers.Magics[FileType.TerrainChunkGeneric],
            FileIdentifiers.Magics[FileType.DdsImage],
            "DSKE"u8.ToArray(),
            "DXBC"u8.ToArray(),
            "GNF"u8.ToArray(),
            "INDR"u8.ToArray(),
            FileIdentifiers.Magics[FileType.Jpeg],
            FileIdentifiers.Magics[FileType.Png],
            FileIdentifiers.Magics[FileType.Riff],
            FileIdentifiers.Magics[FileType.Tome],
            FileIdentifiers.Magics[FileType.Vnfo]
        ];

        DEDICATED_ASSET_HANDLERS =
        [
            (FileIdentifiers.Magics[FileType.MaterialInfo], ScrapeDmat),
            (FileIdentifiers.Magics[FileType.ModelInfo], ScrapeDmod),
            (FileIdentifiers.Magics[FileType.FmodSoundBank5], ScrapeFsb),
            (FileIdentifiers.Magics[FileType.Zone], ScrapeZone)
        ];

        string[] knownFileExtensions =
        [
            "adr", "agr", "Agr", "ags", "apb", "apx", "bat", "bin", "cdt", "cnk0", "cnk1", "cnk2", "cnk3",
            "cnk4", "cnk5", "crc", "crt", "cso", "cur", "Cur", "dat", "Dat", "db", "dds", "DDS", "def", "Def",
            "dir", "Dir", "dll", "DLL", "dma", "dme", "DME", "dmv", "dsk", "dx11efb", "dx11rsb", "dx11ssb",
            "eco", "efb", "exe", "fsb", "fxd", "fxo", "gfx", "gnf", "i64", "ini", "INI", "Ini", "jpg", "JPG",
            "lst", "lua", "mrn", "pak", "pem", "playerstudio", "PlayerStudio", "png", "prsb", "psd", "pssb",
            "tga", "TGA", "thm", "tome", "ttf", "txt", "vnfo", "wav", "xlsx", "xml", "xrsb", "xssb", "zone",
            "Zone"
        ];

        KNOWN_FILE_EXTENSIONS = knownFileExtensions.Select(ext => Encoding.ASCII.GetBytes(ext))
            .ToArray();
    }

    /// <summary>
    /// Scrapes asset names from the given asset's data.
    /// </summary>
    /// <param name="data">The asset data.</param>
    /// <returns>The scraped names.</returns>
    public static IReadOnlyList<string> ScrapeFromAssetData(ReadOnlySpan<byte> data)
    {
        if (!IsScrapeAbleAsset(data))
            return [];

        List<string> names = [];
        ScrapeInternal(data, names);
        int finalCount = names.Count;

        for (int i = 0; i < finalCount; i++)
        {
            string name = names[i];

            if (name.EndsWith(".efb", StringComparison.OrdinalIgnoreCase))
                names.Add(Path.ChangeExtension(name, "dx11efb"));
            else if (name.EndsWith(".xrsb", StringComparison.OrdinalIgnoreCase))
                names.Add(Path.ChangeExtension(name, "dx11rsb"));
            else if (name.EndsWith("xssb", StringComparison.OrdinalIgnoreCase))
                names.Add(Path.ChangeExtension(name, "dx11ssb"));
            else if (name.EndsWith(".mrn", StringComparison.OrdinalIgnoreCase) && !name.Contains("X64", StringComparison.OrdinalIgnoreCase))
                names.Add($"{Path.GetFileNameWithoutExtension(name)}X64.mrn");
        }

        return names;
    }

    /// <summary>
    /// Performs a crude check to determine whether the <paramref name="assetData"/>
    /// is likely to contain scrape-able names.
    /// </summary>
    /// <param name="assetData"></param>
    /// <returns></returns>
    public static bool IsScrapeAbleAsset(ReadOnlySpan<byte> assetData)
    {
        SpanReader<byte> reader = new(assetData);

        foreach (ReadOnlyMemory<byte> value in UNSCRAPEABLE_FILE_MAGICS)
        {
            if (reader.IsNext(value.Span))
                return false;
        }

        return true;

        // FXD files have an offset header, gotta check for them individually
        bool isFxd = assetData.Length > 11
            && assetData[8..].IndexOf("FXD"u8) == 0;
        if (isFxd)
            return true;
    }

    private static void ScrapeInternal(ReadOnlySpan<byte> data, List<string> namesOutput)
    {
        SpanReader<byte> reader = new(data);

        foreach ((ReadOnlyMemory<byte> magic, DedicatedAssetHandler handler) in DEDICATED_ASSET_HANDLERS)
        {
            if (!reader.IsNext(magic.Span))
                continue;

            try
            {
                handler(data, namesOutput);
                return;
            }
            catch
            {
                // We'll simply scrape it naively
            }
        }

        while (!reader.End)
        {
            if (!reader.TryAdvanceTo((byte)'.'))
                break;

            foreach (byte[] extName in KNOWN_FILE_EXTENSIONS)
            {
                if (!reader.IsNext(extName, true))
                    continue;

                int endIndex = reader.Consumed;
                reader.Rewind(extName.Length + 2); // Rewind to the first letter of the name

                // Rewind until we encounter an invalid character, or reach the start of the file
                while (reader.TryPeek(out byte currentChar) && IsValidFileNameChar(currentChar) && reader.Consumed != 0)
                    reader.Rewind(1);

                if (reader.Consumed != 0)
                    reader.Advance(1); // Advance back past the first invalid character encountered above
                int startIndex = reader.Consumed;
                bool read = reader.TryReadExact(out ReadOnlySpan<byte> fullName, endIndex - startIndex);

                // Only include names that aren't just an extension
                if (!read || fullName.IndexOf((byte)'.') == 0)
                    continue;

                string name = Encoding.UTF8.GetString(fullName);
                namesOutput.Add(name);

                // It's possible common for this scrape to capture leading characters (e.g. braces or quotes) that
                // aren't likely to be part of the filename. Hence, remove any non-letter or digit characters
                // and add the name as a variant
                if (!char.IsLetterOrDigit(name[0]))
                    namesOutput.Add(name[1..]);

                // Some datasheet text files contain names with substitutions
                // E.g. ClientItemDefinitions.txt is particularly egregious with this
                if (name.Contains('<'))
                    PerformSubstitutions(name, namesOutput);
            }
        }
    }

    private static void PerformSubstitutions(string name, List<string> namesOutput)
    {
        const string genderKey = "<gender>";
        const string factionKey = "<Faction>";
        const string actorKey = "<Actor>";

        if (name.Contains(genderKey, StringComparison.OrdinalIgnoreCase))
        {
            namesOutput.Add(name.Replace(genderKey, "FEMALE"));
            namesOutput.Add(name.Replace(genderKey, "MALE"));
        }

        if (name.Contains(factionKey, StringComparison.OrdinalIgnoreCase))
        {
            namesOutput.Add(name.Replace(factionKey, "NC"));
            namesOutput.Add(name.Replace(factionKey, "NSO"));
            namesOutput.Add(name.Replace(factionKey, "TR"));
            namesOutput.Add(name.Replace(factionKey, "VS"));
        }

        if (name.Contains(actorKey, StringComparison.OrdinalIgnoreCase))
        {
            // TODO: Fill in actor values
        }
    }

    private static void ScrapeDmat(ReadOnlySpan<byte> dmatData, ICollection<string> namesOutput)
    {
        foreach (string element in Dmat.Read(dmatData, out _).TextureFileNames)
            namesOutput.Add(element);
    }

    private static void ScrapeDmod(ReadOnlySpan<byte> dmodData, ICollection<string> namesOutput)
    {
        SpanReader<byte> reader = new(dmodData);
        bool advanced = reader.TryAdvanceTo(new[] { (byte)'D', (byte)'M', (byte)'A', (byte)'T' }, false);

        if (!advanced)
            return;

        foreach (string element in Dmat.Read(dmodData[reader.Consumed..], out _).TextureFileNames)
            namesOutput.Add(element);
    }

    private static void ScrapeFsb(ReadOnlySpan<byte> fsbData, ICollection<string> namesOutput)
    {
        BinaryPrimitiveReader reader = new(fsbData);
        Fsb5Header header = Fsb5Header.Read(ref reader);

        if (header.NumSamples > 1)
        {
            throw new InvalidDataException
            (
                "FSB5 file with multiple samples encountered. Mandible does not understand how names are packed in " +
                "this case. Please create a bug report and share the FSB file"
            );
        }

        // Skip past the sample headers
        reader.Seek(header.SampleHeaderLen);

        if (reader.ReadInt32LE() != 4)
            throw new InvalidDataException("Unexpected: the name buffer does not start with 0x4");

        ReadOnlySpan<byte> nameBuffer = reader.ReadBytes(header.NameLen - sizeof(int));
        int zeroIndex = nameBuffer.IndexOf((byte)0);
        if (zeroIndex != -1)
            nameBuffer = nameBuffer[..zeroIndex];

        namesOutput.Add(Encoding.ASCII.GetString(nameBuffer) + ".fsb");
    }

    private static void ScrapeZone(ReadOnlySpan<byte> zoneData, ICollection<string> namesOutput)
    {
        Zone.Zone zone = Zone.Zone.Read(zoneData, out _);

        foreach (Eco eco in zone.Ecos)
        {
            namesOutput.Add(eco.TextureInfo.ColorNxMapName);
            namesOutput.Add(eco.TextureInfo.SpecBlendNyMapName);
        }

        foreach (Flora flora in zone.Florae)
        {
            namesOutput.Add(flora.Model);
            namesOutput.Add(flora.Texture);
        }

        foreach (RuntimeObject obj in zone.Objects)
            namesOutput.Add(obj.ActorFile);
    }

    /// <summary>
    /// We use a strict validation check to help limit false matches when parsing binary files.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsValidFileNameChar(byte value)
        => value switch
        {
            >= (byte)'0' and <= (byte)'9' => true,
            >= (byte)'A' and <= (byte)'Z' => true,
            >= (byte)'a' and <= (byte)'z' => true,
            (byte)'-' or (byte)'_' => true,
            (byte)'(' or (byte)')' => true,
            (byte)'[' or (byte)']' => true,
            (byte)'<' or (byte)'>' => true, // Almost always used when substitutions into the name are required
            (byte)'\'' => true,
            (byte)'.' => true, // Periods in name (e.g. my.file.txt)
            _ => false
        };
}
