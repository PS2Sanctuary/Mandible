using MemoryReaders;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Mandible.Locale;

/// <summary>
/// Represents an entry from a locale directory file.
/// </summary>
/// <param name="LookupHash">The Jenkin's Lookup hash of the data entry.</param>
/// <param name="DataEntryByteOffset">The byte offset into the data file at which the entry begins.</param>
/// <param name="DataEntryByteLength">The byte length of the entry.</param>
/// <param name="Unknown">Unknown. Always observed to be 'd'.</param>
public record LocaleDirectoryEntry
(
    uint LookupHash,
    long DataEntryByteOffset,
    int DataEntryByteLength,
    char Unknown = 'd'
)
{
    /// <summary>
    /// Attempts to parse a <see cref="LocaleDirectoryEntry"/> object from a string.
    /// </summary>
    /// <param name="entry">The entry string.</param>
    /// <param name="result">The parsed object, or <c>null</c> if parsing failed.</param>
    /// <returns><c>True</c> if parsing was successful, otherwise <c>false</c>.</returns>
    public static bool TryParse(ReadOnlySpan<char> entry, [NotNullWhen(true)] out LocaleDirectoryEntry? result)
    {
        result = null;
        SpanReader<char> reader = new(entry);

        if (!reader.TryReadTo(out ReadOnlySpan<char> lookupHashStr, '\t'))
            return false;
        if (!uint.TryParse(lookupHashStr, out uint lookupHash))
            return false;

        if (!reader.TryReadTo(out ReadOnlySpan<char> offsetStr, '\t'))
            return false;
        if (!long.TryParse(offsetStr, out long offset))
            return false;

        if (!reader.TryReadTo(out ReadOnlySpan<char> lengthStr, '\t'))
            return false;
        if (!int.TryParse(lengthStr, out int length))
            return false;

        if (!reader.TryRead(out char unknown))
            return false;

        result = new LocaleDirectoryEntry(lookupHash, offset, length, unknown);
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{LookupHash}\t{DataEntryByteOffset}\t{DataEntryByteLength}\t{Unknown}";
}
