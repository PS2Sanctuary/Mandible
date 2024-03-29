using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mandible.Locale;

/// <summary>
/// Contains utility methods for interacting with locale files.
/// </summary>
public static class LocaleHelpers
{
    /// <summary>
    /// Creates the header of a locale directory file.
    /// </summary>
    /// <param name="count">The number of locale entries.</param>
    /// <param name="date">The date and time at which the locale directory was generated.</param>
    /// <param name="game">The game in which the locale directory will be used.</param>
    /// <param name="locale">The language identifier of the locale.</param>
    /// <param name="md5Checksum">An MD5 checksum of the associated locale data file.</param>
    /// <param name="textLength">
    /// The length of the longest content string in the associated <see cref="LocaleDataEntry"/> objects.
    /// </param>
    /// <param name="cidLength">Unknown.</param>
    /// <param name="t4Version">Unknown. Presumably T4 templates are used internally to generate locale files.</param>
    /// <param name="version">Unknown.</param>
    /// <returns>The header string.</returns>
    public static string CreateLocaleDirectoryHeader
    (
        int count,
        DateTimeOffset date,
        string game,
        string locale,
        IEnumerable<byte> md5Checksum,
        int textLength,
        int cidLength = 193,
        string t4Version = "Unknown",
        string version = "2.1.886508"
    )
    {
        StringBuilder sb = new();
        sb.Append("## CidLength:\t").AppendLine(cidLength.ToString())
            .Append("## Count:\t").AppendLine(count.ToString())
            .Append("## Date:\t").AppendLine(date.ToString("ddd MMM dd HH:mm:ss yyyy"))
            .Append("## Game:\t").AppendLine(game)
            .Append("##Locale:\t").AppendLine(locale.ToLower())
            .Append("## MD5Checksum: ");

        foreach (byte value in md5Checksum)
            sb.Append($"{value:X2}");

        sb.AppendLine()
            .Append("## T4Version:\t").AppendLine(t4Version)
            .Append("## TextLength:\t").AppendLine(textLength.ToString())
            .Append("## Version:\t").Append(version);

        return sb.ToString();
    }

    /// <summary>
    /// Writes locale data.
    /// </summary>
    /// <param name="dataEntries">The data entries to write.</param>
    /// <param name="languageIdentifier">The language of the entries.</param>
    /// <param name="gameIdentifier">The game that the locale is constructed for.</param>
    /// <param name="localeDataOutput">A stream to write the data entries to.</param>
    /// <param name="localeDirectoryOutput">A stream to write the directory entries to.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="localeDataOutput"/> is not seekable, or at the start.
    /// </exception>
    public static void WriteLocaleData
    (
        IEnumerable<LocaleDataEntry> dataEntries,
        string languageIdentifier,
        string gameIdentifier,
        Stream localeDataOutput,
        Stream localeDirectoryOutput
    )
    {
        if (!localeDataOutput.CanSeek)
            throw new ArgumentException($"The data stream must be seekable", nameof(localeDataOutput));

        if (localeDataOutput.Position is not 0)
            throw new ArgumentException("The data stream must be at the beginning", nameof(localeDataOutput));

        localeDataOutput.Write(Encoding.UTF8.Preamble);

        using StreamWriter dataWriter = new(localeDataOutput, new UTF8Encoding(true), leaveOpen: true);
        using StreamWriter directoryWriter = new(localeDirectoryOutput, new UTF8Encoding(false), leaveOpen: true);

        List<LocaleDataEntry> sortedEntries = new(dataEntries);
        sortedEntries.Sort((x1, x2) => x1.LookupHash.CompareTo(x2.LookupHash));

        List<LocaleDirectoryEntry> directoryEntries = new(sortedEntries.Count);
        int longestContent = 0;

        foreach (LocaleDataEntry entry in sortedEntries)
        {
            if (entry.Content.Length > longestContent)
                longestContent = entry.Content.Length;

            long offset = localeDataOutput.Position;
            dataWriter.WriteLine(entry.ToString());
            dataWriter.Flush(); // We need to flush to ensure we get correct offsets/lengths

            LocaleDirectoryEntry dirEntry = new
            (
                entry.LookupHash,
                offset,
                (int)(localeDataOutput.Position - offset) - 2 // - 2 for line break
            );
            directoryEntries.Add(dirEntry);
        }

        localeDataOutput.Seek(0, SeekOrigin.Begin);
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(localeDataOutput);

        string directoryHeader = CreateLocaleDirectoryHeader
        (
            sortedEntries.Count,
            DateTimeOffset.UtcNow,
            gameIdentifier,
            languageIdentifier.ToLower(),
            hash,
            longestContent
        );

        directoryWriter.WriteLine(directoryHeader);
        foreach (LocaleDirectoryEntry entry in directoryEntries)
            directoryWriter.WriteLine(entry.ToString());
    }

    /// <summary>
    /// Reads locale data.
    /// </summary>
    /// <param name="localeDirectory">A stream containing the locale directory.</param>
    /// <param name="localeData">A stream containing the locale data.</param>
    /// <returns>The read <see cref="LocaleDataEntry"/> objects.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="localeData"/> does not contain a BOM marker.
    /// </exception>
    /// <exception cref="FormatException">Thrown if either input stream contains invalid formatting.</exception>
    public static List<LocaleDataEntry> ReadLocaleData(Stream localeDirectory, Stream localeData)
    {
        byte[] lineBreak = { (byte)'\r', (byte)'\n' };
        using StreamReader directoryReader = new(localeDirectory, Encoding.UTF8, leaveOpen: true);
        List<LocaleDataEntry> dataEntries = new();

        Span<byte> bom = stackalloc byte[Encoding.UTF8.Preamble.Length];
        int bomReadLen = localeData.Read(bom);
        if (bomReadLen != Encoding.UTF8.Preamble.Length || !bom.SequenceEqual(Encoding.UTF8.Preamble))
            throw new ArgumentException("Data stream does not contain a BOM", nameof(localeData));

        while (!directoryReader.EndOfStream)
        {
            string? line = directoryReader.ReadLine();
            if (line is null)
                break;

            if (!LocaleDirectoryEntry.TryParse(line, out LocaleDirectoryEntry? directoryEntry))
                continue;

            int expectedLength = directoryEntry.DataEntryByteLength + 2; // + 2 for line break
            byte[] buffer = ArrayPool<byte>.Shared.Rent(expectedLength); // + 2 for line break
            Span<byte> data = buffer.AsSpan(0, expectedLength);

            int amountRead = localeData.Read(data);
            if (amountRead != expectedLength)
                throw new FormatException("Failed to read the expected number of characters from the data stream");
            if (!data.EndsWith(lineBreak))
                throw new FormatException("Data entry does not end with line break");

            string value = Encoding.UTF8.GetString(data[..^2]);
            if (LocaleDataEntry.TryParse(value, out LocaleDataEntry? entry))
                dataEntries.Add(entry);
        }

        return dataEntries;
    }
}
