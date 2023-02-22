using MemoryReaders;
using System;

namespace Mandible.Locale;

public record LocaleDirectoryEntry
(
    uint LookupHash,
    uint DataEntryOffset,
    ushort DataEntryLength,
    char Unknown = 'd'
)
{
    public static bool TryParse(ReadOnlySpan<char> entryLine, out LocaleDirectoryEntry? entry)
    {
        entry = null;
        SpanReader<char> reader = new(entryLine);

        if (!reader.TryReadTo(out ReadOnlySpan<char> lookupHashStr, '\t'))
            return false;
        if (!uint.TryParse(lookupHashStr, out uint lookupHash))
            return false;

        if (!reader.TryReadTo(out ReadOnlySpan<char> offsetStr, '\t'))
            return false;
        if (!uint.TryParse(offsetStr, out uint offset))
            return false;

        if (!reader.TryReadTo(out ReadOnlySpan<char> lengthStr, '\t'))
            return false;
        if (!ushort.TryParse(lengthStr, out ushort length))
            return false;

        if (!reader.TryRead(out char unknown))
            return false;

        entry = new LocaleDirectoryEntry(lookupHash, offset, length, unknown);
        return true;
    }
}
