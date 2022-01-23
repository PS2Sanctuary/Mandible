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

namespace Mandible.Pack2.Names;

/// <summary>
/// Provides functionalities to extract a namelist from pack2 files.
/// </summary>
public static class NameExtractor
{
    /// <summary>
    /// Gets the CRC-64 hash of the namelist file name ( {NAMELIST} ).
    /// </summary>
    private const ulong NamelistFileNameHash = 4699449473529019696;

    private static readonly byte[] IllegalNameCharacters = new char[] { '!', '"', '#', '$', '%', '&', '*', '+', ',', '/', ':', ';', '=', '>', '?', '@', '\\', '^', '`', '|', '~', '\t', '\r', '\n', ' ' }.Select(c => (byte)c).ToArray();
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

        int length = await reader.GetAssetLengthAsync(namelistHeader.Value, ct).ConfigureAwait(false);
        using IMemoryOwner<byte> buffer = MemoryPool<byte>.Shared.Rent(length);
        await reader.ReadAssetDataAsync(namelistHeader.Value, buffer.Memory[..length], ct).ConfigureAwait(false);

        byte[] dataArray = ArrayPool<byte>.Shared.Rent(length);
        buffer.Memory[..length].CopyTo(dataArray);

        await using MemoryStream ms = new(dataArray);
        await namelist.Append(ms, length, ct).ConfigureAwait(false);
        ArrayPool<byte>.Shared.Return(dataArray);
    }

    private static async Task ExtractFromAssetsAsync(IPack2Reader reader, Namelist namelist, CancellationToken ct)
    {
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        foreach (Asset2Header asset in assetHeaders)
        {
            int length = await reader.GetAssetLengthAsync(asset, ct).ConfigureAwait(false);
            using IMemoryOwner<byte> buffer = MemoryPool<byte>.Shared.Rent(length);
            await reader.ReadAssetDataAsync(asset, buffer.Memory[..length], ct).ConfigureAwait(false);

            if (IsBinaryFile(buffer.Memory.Span[..length]))
                continue;

            List<string> names = ExtractNamesFromTextData(buffer.Memory[..length]);
            await namelist.Append(names, ct).ConfigureAwait(false);
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
