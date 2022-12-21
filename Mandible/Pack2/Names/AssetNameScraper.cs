using Mandible.Dma;
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
    private delegate IReadOnlyList<string> DedicatedAssetHandler(ReadOnlySpan<byte> assetData);

    private static readonly IReadOnlyList<byte[]> UNSCRAPEABLE_FILE_MAGICS;
    private static readonly IReadOnlyList<byte[]> SCRAPEABLE_BINARY_FILE_MAGICS;
    private static readonly IReadOnlyList<(byte[] Magic, DedicatedAssetHandler Handler)> DEDICATED_ASSET_HANDLERS;
    private static readonly IReadOnlyList<byte[]> KNOWN_FILE_EXTENSIONS;

    static AssetNameScraper()
    {
        UNSCRAPEABLE_FILE_MAGICS = new[]
        {
            Encoding.ASCII.GetBytes("CDTA"),
            Encoding.ASCII.GetBytes("CFX"),
            Encoding.ASCII.GetBytes("CNK"),
            Encoding.ASCII.GetBytes("DDS"),
            Encoding.ASCII.GetBytes("DSKE"),
            Encoding.ASCII.GetBytes("DXBC"),
            Encoding.ASCII.GetBytes("FSB"),
            Encoding.ASCII.GetBytes("GNF"),
            Encoding.ASCII.GetBytes("INDR"),
            Encoding.ASCII.GetBytes("RIFF"),
            Encoding.ASCII.GetBytes("VNFO"),
            new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G' },
            new byte[] { 0xff, 0xd8, 0xff } // JPG
        };

        SCRAPEABLE_BINARY_FILE_MAGICS = new[]
        {
            Encoding.ASCII.GetBytes("CHKF"),
            Encoding.ASCII.GetBytes("DMAT"),
            Encoding.ASCII.GetBytes("DMOD"),
            Encoding.ASCII.GetBytes("ZONE"),
            Encoding.ASCII.GetBytes("*TEXTUREPART") // .eco
        };

        DEDICATED_ASSET_HANDLERS = new (byte[], DedicatedAssetHandler)[]
        {
            (Encoding.ASCII.GetBytes("DMAT"), ScrapeDMAT),
            (Encoding.ASCII.GetBytes("DMOD"), ScrapeDMOD)
        };

        string[] knownFileExtensions =
        {
            "adr", "agr", "ags", "apb", "apx", "bat", "bin", "cdt", "cnk0", "cnk1", "cnk2", "cnk3",
            "cnk4", "cnk5", "crc", "crt", "cso", "cur", "dat", "db", "dds", "def", "dir", "dll",
            "dma", "dme", "dmv", "dsk", "dx11efb", "dx11rsb", "dx11ssb", "eco", "efb", "exe", "fsb",
            "fxd", "fxo", "gfx", "gnf", "i64", "ini", "jpg", "lst", "lua", "mrn", "pak", "pem",
            "playerstudio", "png", "prsb", "psd", "pssb", "tga", "thm", "tome", "ttf", "txt", "vnfo",
            "wav", "xlsx", "xml", "xrsb", "xssb", "zone"
        };

        KNOWN_FILE_EXTENSIONS = knownFileExtensions.Select(ext => Encoding.ASCII.GetBytes(ext))
            .ToList();
    }

    /// <summary>
    /// Scrapes asset names from the given asset's data.
    /// </summary>
    /// <param name="data">The asset data.</param>
    /// <returns>The scraped names.</returns>
    public static IReadOnlyList<string> ScrapeFromAssetData(ReadOnlySpan<byte> data)
    {
        if (!IsScrapeAbleAsset(data))
            return Array.Empty<string>();

        IReadOnlyList<string> names = ScrapeInternal(data);

        List<string> returnList = new();
        foreach (string name in names)
        {
            if (name.EndsWith(".efb"))
            {
                returnList.Add(Path.ChangeExtension(name, "dx11efb"));
            }
            else if (name.EndsWith(".xrsb"))
            {
                returnList.Add(Path.ChangeExtension(name, "dx11rsb"));
            }
            else if (name.EndsWith("xssb"))
            {
                returnList.Add(Path.ChangeExtension(name, "dx11ssb"));
            }
            else if (name.EndsWith(".mrn") && !name.Contains("X64"))
            {
                string fileName = Path.GetFileNameWithoutExtension(name);
                returnList.Add($"{fileName}X64.mrn");
            }

            returnList.Add(name);
        }

        return returnList;
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

        // FXD files have an offset header, gotta check for them individually
        bool isFxd = assetData.Length > 11
            && assetData[8..].IndexOf(Encoding.UTF8.GetBytes("FXD")) == 0;
        if (isFxd)
            return true;

        foreach (byte[] value in SCRAPEABLE_BINARY_FILE_MAGICS)
        {
            if (reader.IsNext(value))
                return true;
        }

        foreach (byte[] value in UNSCRAPEABLE_FILE_MAGICS)
        {
            if (reader.IsNext(value))
                return false;
        }

        // Simple check for binary files
        int maxCheckLength = Math.Min(2048, assetData.Length);
        return maxCheckLength > 0
            && assetData[..maxCheckLength].IndexOf((byte)0) == -1;
    }

    private static IReadOnlyList<string> ScrapeInternal(ReadOnlySpan<byte> data)
    {
        SpanReader<byte> reader = new(data);

        foreach ((byte[] magic, DedicatedAssetHandler handler) in DEDICATED_ASSET_HANDLERS)
        {
            if (reader.IsNext(magic))
                return handler(data);
        }

        List<string> names = new();
        while (!reader.End)
        {
            if (!reader.TryAdvanceTo((byte)'.'))
                break;

            for (int i = 0; i < KNOWN_FILE_EXTENSIONS.Count; i++)
            {
                byte[] extName = KNOWN_FILE_EXTENSIONS[i];
                if (!reader.IsNext(extName, true))
                    continue;

                int endIndex = reader.Consumed;
                reader.Rewind(extName.Length + 2); // Rewind to the first letter of the name

                // Rewind until we encounter an invalid character, or reach the start of the file
                while (reader.TryPeek(out byte currentChar) && IsValidLetter(currentChar) && reader.Consumed != 0)
                    reader.Rewind(1);

                if (reader.Consumed != 0)
                    reader.Advance(1); // Advance back past the first invalid character encountered above
                int startIndex = reader.Consumed;
                bool read = reader.TryReadExact(out ReadOnlySpan<byte> fullName, endIndex - startIndex);

                // Only include names that aren't just an extension
                if (read && fullName.IndexOf((byte)'.') != 0)
                    names.Add(Encoding.UTF8.GetString(fullName));

                reader.Advance(endIndex - startIndex);
            }
        }

        return names;
    }

    private static IReadOnlyList<string> ScrapeDMAT(ReadOnlySpan<byte> dmatData)
        => Dmat.Read(dmatData, out _).TextureFileNames;

    private static IReadOnlyList<string> ScrapeDMOD(ReadOnlySpan<byte> dmodData)
    {
        SpanReader<byte> reader = new(dmodData);
        bool advanced = reader.TryAdvanceTo(new[] { (byte)'D', (byte)'M', (byte)'A', (byte)'T' }, false);

        return advanced
            ? Dmat.Read(dmodData[reader.Consumed..], out _).TextureFileNames
            : Array.Empty<string>();
    }

    private static bool IsValidLetter(byte value)
        => value switch
        {
            >= (byte)'0' and <= (byte)'9' => true,
            >= (byte)'A' and <= (byte)'Z' => true,
            >= (byte)'a' and <= (byte)'z' => true,
            (byte)'-' => true,
            (byte)'_' => true,
            _ => false
        };
}
