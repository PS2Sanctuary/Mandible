using BinaryPrimitiveHelpers;
using Mandible.Common;
using Mandible.Dma;
using Mandible.Fsb;
using Mandible.Zone;
using MemoryReaders;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mandible.Pack2.Names;

/// <summary>
/// A utility class for scraping asset names from PACK2 asset data.
/// </summary>
public static partial class AssetNameScraper
{
    private delegate void DedicatedAssetHandler(ReadOnlySpan<byte> assetData, List<string> namesOutput);

    private static readonly SearchValues<char> INVALID_FILE_NAME_CHARS;

    // A list of file extensions that are known to appear in the asset data and result in valid file names.
    // Some assets do contain names/extensions from source files which are not included in the asset output, so we
    // don't list those here either
    private static readonly IReadOnlyList<byte[]> KNOWN_FILE_EXTENSIONS;
    private static readonly Dictionary<FileType, DedicatedAssetHandler> _dedicatedAssetHandlers;

    static AssetNameScraper()
    {
        INVALID_FILE_NAME_CHARS = SearchValues.Create(Path.GetInvalidFileNameChars());

        _dedicatedAssetHandlers = new Dictionary<FileType, DedicatedAssetHandler>
        {
            { FileType.ActorDefinition, ScrapeAdr },
            { FileType.Eco, ScrapeEco },
            { FileType.EfbDx11_Model4, ScrapeEfbDx11 },
            { FileType.EfbDx11_Model5, ScrapeEfbDx11 },
            { FileType.Gfx, ScrapeGfx },
            { FileType.Fxo, ScrapeFxo },
            { FileType.MaterialInfo, ScrapeDmat },
            { FileType.ModelInfo, ScrapeDmod },
            { FileType.MorphemeRuntimeNetwork, ScrapeMrn },
            { FileType.MorphemeRuntimeNetwork64Bit, ScrapeMrn },
            { FileType.FmodSoundBank5, ScrapeFsb },
            { FileType.Zone, ScrapeZone }
        };

        // TODO: There are probably extensions that we ONLY need to scrape from certain file formats,
        // This may help us to speed up the scrape? Benchmark
        string[] knownFileExtensions =
        [
            "adr", "agr", "Agr", "ags", "apb", "cnk0", "cnk1", "cnk2", "cnk3", "cnk4", "cnk5", "crt", "cso",
            "cur", "Cur", "db", "dds", "DDS", "def", "Def", "dma", "dme", "DME", "dmv", "dsk", "dx11efb", "dx11rsb",
            "dx11ssb", "eco", "efb", "fsb", "fxd", "fxo", "gfx", "gnf", "ini", "INI", "Ini", "jpg", "JPG", "lst",
            "lua", "mrn", "pem", "playerstudio", "PlayerStudio", "png", "prsb", "pssb", "swf", "tga", "TGA", "tome",
            "txt", "vnfo", "wav", "xml", "xrsb", "xssb", "zone", "Zone"
        ];
        KNOWN_FILE_EXTENSIONS = knownFileExtensions.Select(ext => Encoding.ASCII.GetBytes("." + ext))
            .ToArray();
    }

    /// <summary>
    /// Scrapes asset names from the given asset's data.
    /// </summary>
    /// <param name="data">The asset data.</param>
    /// <returns>The scraped names.</returns>
    public static IReadOnlyList<string> ScrapeFromAssetData(ReadOnlySpan<byte> data)
    {
        FileType type = FileIdentifiers.InferFileType(data);

        if (!IsScrapeableAsset(type))
            return [];

        List<string> names = [];
        ScrapeInternal(type, data, names);
        int finalCount = names.Count;

        // Post-process to clean up the scraped names and apply patterns
        for (int i = 0; i < finalCount; i++)
        {
            string name = names[i];

            // Remove directory info
            if (name.Contains('\\') || name.Contains('/'))
                names[i] = name = Path.GetFileName(name);

            // Remove invalid file name characters
            if (name.AsSpan().ContainsAny(INVALID_FILE_NAME_CHARS))
            {
                char[] newName = ArrayPool<char>.Shared.Rent(name.Length);
                int nameIndex = 0;
                foreach (char element in name)
                {
                    if (!INVALID_FILE_NAME_CHARS.Contains(element))
                        newName[nameIndex++] = element;
                }
                names[i] = name = new string(newName, 0, nameIndex);
            }

            AddGlobalFileNamePatterns(name, names);
        }

        return names;
    }

    /// <summary>
    /// Applies various patterns based on the given file name.
    /// Only patterns that are likely to apply at a global level are performed here. Certain file formats may
    /// do their own patterning which is appropriate for the context, but would otherwise result in too many false
    /// positives.
    /// </summary>
    /// <param name="name">The name to process.</param>
    /// <param name="names">The list of names to add the pattern outputs to.</param>
    public static void AddGlobalFileNamePatterns(string name, List<string> names)
    {
        // <CollisionData fileName=""> tags often reference a CDT file with the same name as the ADR file
        if (name.EndsWith(".cdt", StringComparison.OrdinalIgnoreCase))
            names.Add(Path.ChangeExtension(name, "adr"));
        // efb files have DX11 variants
        else if (name.EndsWith(".efb", StringComparison.OrdinalIgnoreCase))
            names.Add(Path.ChangeExtension(name, "dx11efb"));
        // FSB faction sounds have NSO equivalents
        else if (name.EndsWith("_NS.fsb", StringComparison.OrdinalIgnoreCase))
            names.Add(name.Replace("_NS.fsb", "_NSO.fsb", StringComparison.OrdinalIgnoreCase));
        // Morpheme animation files have 64-bit variants
        else if (name.EndsWith(".mrn", StringComparison.OrdinalIgnoreCase) && !name.Contains("X64", StringComparison.OrdinalIgnoreCase))
            names.Add($"{Path.GetFileNameWithoutExtension(name)}X64.mrn");
        // Forgelight games use SWF files in GFX mode
        else if (name.EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
            names.Add(Path.ChangeExtension(name, "gfx"));
        // xrsb files have DX11 variants
        else if (name.EndsWith(".xrsb", StringComparison.OrdinalIgnoreCase))
            names.Add(Path.ChangeExtension(name, "dx11rsb"));
        // xssb files have DX11 variants
        else if (name.EndsWith(".xssb", StringComparison.OrdinalIgnoreCase))
            names.Add(Path.ChangeExtension(name, "dx11ssb"));
    }

    public static bool IsScrapeableAsset(FileType type)
    {
        bool failsTypeCheck = type is FileType.ApexXml
            or FileType.CollisionData
            or FileType.Dske
            or FileType.Dxbc
            or FileType.Gnf
            or FileType.DdsImage
            or FileType.IndoorData
            or FileType.Jpeg
            or FileType.Png
            or FileType.Riff
            or FileType.TerrainChunkLod0
            or FileType.TerrainChunkLod1
            or FileType.TerrainChunkLod2
            or FileType.TerrainChunkLod3
            or FileType.Tome
            or FileType.TruevisionTga
            or FileType.Vnfo;

        return !failsTypeCheck;
    }

    private static void ScrapeInternal(FileType type, ReadOnlySpan<byte> data, List<string> namesOutput)
    {
        if (_dedicatedAssetHandlers.TryGetValue(type, out DedicatedAssetHandler? handler))
        {
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

        ScrapeUnstructuredData(data, namesOutput);
    }

    private static int ScrapeUnstructuredData
    (
        ReadOnlySpan<byte> data,
        List<string> namesOutput
    )
    {
        int scraped = 0;

        foreach (byte[] extName in KNOWN_FILE_EXTENSIONS)
            scraped += ScrapeUnstructuredDataForExtension(data, namesOutput, extName, false);

        return scraped;
    }

    private static int ScrapeUnstructuredDataForExtension
    (
        ReadOnlySpan<byte> data,
        List<string> namesOutput,
        ReadOnlySpan<byte> extNameWithPeriod,
        bool allowDirectorySeparators
    )
    {
        SpanReader<byte> reader = new(data);
        int scraped = 0;

        while (reader.TryAdvanceTo(extNameWithPeriod, advancePastDelimiter: true))
        {
            int endIndex = reader.Consumed;
            reader.Rewind(extNameWithPeriod.Length + 2); // Rewind to the first letter of the name

            // Rewind until we encounter an invalid character, or reach the start of the file
            while (reader.TryPeek(out byte currentChar)
                   && IsValidFileNameChar(currentChar, allowDirectorySeparators)
                   && reader.Consumed != 0)
                reader.Rewind(1);

            if (reader.Consumed != 0)
                reader.Advance(1); // Advance back past the first invalid character encountered above
            int startIndex = reader.Consumed;
            bool read = reader.TryReadExact(out ReadOnlySpan<byte> fullName, endIndex - startIndex);

            // Only include names that aren't just an extension
            if (!read || fullName.IndexOf((byte)'.') == 0)
                continue;

            string name = Encoding.UTF8.GetString(fullName);

            // Some datasheet text files contain names with substitutions. Ignore the original as '<' and '>' are
            // invalid in paths.
            // E.g. ClientItemDefinitions.txt is particularly egregious with this
            if (name.Contains('<') && name.Contains('>'))
            {
                scraped += PerformSubstitutions(name, namesOutput);
            }
            else
            {
                namesOutput.Add(name);
                scraped++;
            }

            // It's somewhat common for this scrape to capture leading characters (e.g. braces or quotes) that
            // aren't likely to be part of the filename. Hence, remove any non-letter or digit characters
            // and add the name as a variant
            if (!char.IsLetterOrDigit(name[0]))
            {
                namesOutput.Add(name[1..]);
                scraped++;
            }
        }

        return scraped;
    }

    private static int PerformSubstitutions(string name, List<string> namesOutput)
    {
        const string genderKey = "<gender>";
        const string factionKey = "<Faction>";
        const string actorKey = "<Actor>";

        if (name.Contains(genderKey, StringComparison.OrdinalIgnoreCase))
        {
            namesOutput.Add(name.Replace(genderKey, "FEMALE"));
            namesOutput.Add(name.Replace(genderKey, "MALE"));
            return 2;
        }

        if (name.Contains(factionKey, StringComparison.OrdinalIgnoreCase))
        {
            namesOutput.Add(name.Replace(factionKey, "NC"));
            namesOutput.Add(name.Replace(factionKey, "NSO"));
            namesOutput.Add(name.Replace(factionKey, "TR"));
            namesOutput.Add(name.Replace(factionKey, "VS"));
            return 4;
        }

        if (name.Contains(actorKey, StringComparison.OrdinalIgnoreCase))
        {
            // TODO: Fill in actor values
        }

        return 0;
    }

    private static void ScrapeAdr(ReadOnlySpan<byte> adrData, List<string> namesOutput)
    {
        Regex dmeToAdrPattern = Regex_ScrapeAdr_DmeToAdrPattern();

        // To date, these files are only referenced from ADRs
        ScrapeUnstructuredDataForExtension(adrData, namesOutput, ".apx"u8, false);
        ScrapeUnstructuredDataForExtension(adrData, namesOutput, ".cdt"u8, false);
        ScrapeUnstructuredDataForExtension(adrData, namesOutput, ".ind"u8, false);

        ScrapeUnstructuredData(adrData, namesOutput);
        int finalCount = namesOutput.Count;

        for (int i = 0; i < finalCount; i++)
        {
            string name = namesOutput[i];

            // File name and Palette name elements (in particular) often match actor file names, once
            // you strip the LOD specifier.
            if (name.EndsWith(".dme", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".dma", StringComparison.OrdinalIgnoreCase))
                namesOutput.Add(dmeToAdrPattern.Replace(name, ".adr"));
        }
    }

    private static void ScrapeEco(ReadOnlySpan<byte> ecoData, List<string> namesOutput)
    {
        SpanReader<byte> reader = new(ecoData);

        do
        {
            // Skip any tabs before nested properties
            while (reader.TryPeek(out byte value) && value is (byte)'\t')
                reader.Advance(1);

            // Note the specific tab here, to prevent issues with property names that have the same starting prefix
            // (notably ClumpMask).
            bool takeDirectName = reader.IsNext("*SOURCE_COLOR_BLEND_MAP\t"u8, advancePast: true)
                || reader.IsNext("*SOURCE_NORMAL_MAP\t"u8, advancePast: true)
                || reader.IsNext("*SOURCE_SPEC_MAP\t"u8, advancePast: true)
                || reader.IsNext("*RENDER_COLOR_NX_MAP\t"u8, advancePast: true)
                || reader.IsNext("*RENDER_SPEC_BLEND_NY_MAP\t"u8, advancePast: true)
                || reader.IsNext("*CLUMPMASK\t"u8, advancePast: true);
            bool takeEcoName = reader.IsNext("*TEXTURELAYER\t"u8, advancePast: true);

            ReadOnlySpan<byte> nameBytes = ReadOnlySpan<byte>.Empty;
            if (takeDirectName || takeEcoName)
                reader.TryReadTo(out nameBytes, (byte)'\r', advancePastDelimiter: false);
            else
                continue;

            string name = Encoding.UTF8.GetString(nameBytes);
            name = name.Trim('"'); // ClumpMask is quoted

            if (takeDirectName)
            {
                namesOutput.Add(name);

                // Removing the texture type denominator often gives the name of the eco file
                // e.g. indar_ocean_pebbles_c.dds -> indar_ocean_pebbles.eco
                int lastFragment = name.LastIndexOf('_');
                if (lastFragment != -1)
                {
                    string tempName = name[..lastFragment];
                    namesOutput.Add(tempName + ".eco");
                }
            }
            else if (takeEcoName)
            {
                namesOutput.Add(name + ".eco");
            }
        }
        while (reader.TryAdvanceTo("\r\n"u8, advancePastDelimiter: true));
    }

    private static void ScrapeEfbDx11(ReadOnlySpan<byte> efbData, List<string> namesOutput)
    {
        const int nameOffset = 0x78;

        if (efbData.Length < nameOffset)
            return;

        efbData = efbData[nameOffset..];
        int nameEnd = efbData.IndexOf((byte)0);
        namesOutput.Add(Encoding.ASCII.GetString(efbData[..nameEnd]) + ".dx11efb");
    }

    private static unsafe void ScrapeGfx(ReadOnlySpan<byte> gfxData, List<string> namesOutput)
    {
        bool notValid = gfxData.Length < 10 // Eight header bytes + 2 zlib header bytes
            //|| gfxData[3] is not (12 or 37) // Byte 4 is the version. We only know that we understand 12 and 37
            || gfxData[8] is not 0x78; // Naive zlib check - ensure another compression alg isn't in use.

        if (notValid)
        {
            ScrapeUnstructuredData(gfxData, namesOutput);
            Debug.Assert(false, "Invalid GFX data encountered");
            return;
        }

        // The eighth byte onwards is zlib-compressed data
        ReadOnlySpan<byte> compressedData = gfxData[8..];
        using MemoryStream msOut = new(gfxData.Length * 2);

        fixed (byte* ptr = compressedData)
        {
            using UnmanagedMemoryStream ums = new(ptr, compressedData.Length);
            using ZLibStream zs = new(ums, CompressionMode.Decompress);
            zs.CopyTo(msOut);
        }

        ReadOnlySpan<byte> decompressed = msOut.GetBuffer().AsSpan(0, (int)msOut.Position);
        ScrapeUnstructuredData(decompressed, namesOutput);
    }

    private static void ScrapeFxo(ReadOnlySpan<byte> fxoData, List<string> namesOutput)
    {
        // FXO files reference the source fx and fxh files.
        // As "fxh" includes "fx", we capture both with the below scrape.
        ScrapeUnstructuredDataForExtension(fxoData, namesOutput, ".fx"u8, false);
        for (int i = 0; i < namesOutput.Count; i++)
            namesOutput[i] += "o";

        // PlanetSide's shaders often reference DDS and TGA images, but a lot more is possible to be used in shader dev,
        // so do a full unstructured scrape.
        ScrapeUnstructuredData(fxoData, namesOutput);
    }

    private static void ScrapeDmat(ReadOnlySpan<byte> dmatData, List<string> namesOutput)
    {
        foreach (string element in Dmat.Read(dmatData, out _).TextureFileNames)
            namesOutput.Add(element);
    }

    private static void ScrapeDmod(ReadOnlySpan<byte> dmodData, List<string> namesOutput)
    {
        SpanReader<byte> reader = new(dmodData);
        bool advanced = reader.TryAdvanceTo(FileIdentifiers.Magics[FileType.MaterialInfo].Span, false);

        if (!advanced)
            return;

        foreach (string element in Dmat.Read(dmodData[reader.Consumed..], out _).TextureFileNames)
            namesOutput.Add(element);
    }

    private static void ScrapeMrn(ReadOnlySpan<byte> mrnData, List<string> namesOutput)
    {
        // TODO: Scrape fbx paths (note different extension for 32-bit MRNs)
        // and extract info from the path to create MRN file names.
        // Relevant file on which this works is DomeShieldEmitterX64.mrn
        ScrapeUnstructuredData(mrnData, namesOutput);
    }

    private static void ScrapeFsb(ReadOnlySpan<byte> fsbData, List<string> namesOutput)
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

    private static void ScrapeZone(ReadOnlySpan<byte> zoneData, List<string> namesOutput)
    {
        Zone.Zone zone = Zone.Zone.Read(zoneData, out _);

        foreach (Eco eco in zone.Ecos)
        {
            namesOutput.Add(eco.TextureInfo.Name + ".eco");
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
    /// <param name="value">The character to check.</param>
    /// <param name="allowDirectorySeparators">Whether directory separators should be considered as valid.</param>
    /// <returns>Whether the value is a valid filename character.</returns>
    private static bool IsValidFileNameChar(byte value, bool allowDirectorySeparators)
        => value switch
        {
            >= (byte)'0' and <= (byte)'9' => true,
            >= (byte)'A' and <= (byte)'Z' => true,
            >= (byte)'a' and <= (byte)'z' => true,
            (byte)'-' or (byte)'_' => true,
            (byte)'(' or (byte)')' => true, // TODO: Experiment with removing
            (byte)'[' or (byte)']' => true, // TODO: Experiment with removing
            (byte)'<' or (byte)'>' => true, // Almost always used when substitutions into the name are required
            (byte)'\'' => true, // TODO: Experiment with removing
            (byte)'.' => true, // Periods in name (e.g. my.file.txt)
            (byte)'\\' or (byte)'/' when allowDirectorySeparators => true,
            _ => false
        };

    // This Regex strictly matches any values that end in either .dma or .dme.
    // This is captured so that the extension can be changed.
    // Further, it matches fragments in the format _LOD[0-9][Auto], so they can be removed.
    [GeneratedRegex(@"(?:_LOD\d?(?:Auto)?)*(\.dm[ea])", RegexOptions.IgnoreCase)]
    private static partial Regex Regex_ScrapeAdr_DmeToAdrPattern();
}
