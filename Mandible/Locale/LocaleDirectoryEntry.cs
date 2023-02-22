using MemoryReaders;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Mandible.Locale;

public record LocaleDirectoryEntry
(
    uint LookupHash,
    long DataEntryOffset,
    int DataEntryByteLength,
    char Unknown = 'd'
)
{
    public static bool TryParse(ReadOnlySpan<char> entryLine, [NotNullWhen(true)] out LocaleDirectoryEntry? entry)
    {
        entry = null;
        SpanReader<char> reader = new(entryLine);

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

        entry = new LocaleDirectoryEntry(lookupHash, offset, length, unknown);
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{LookupHash}\t{DataEntryOffset}\t{DataEntryByteLength}\t{Unknown}";
}
