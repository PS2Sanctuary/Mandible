using Mandible.Util;
using MemoryReaders;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Mandible.Locale;

public class LocaleDataEntry
{
    public const string TypeUCDT = "ucdt";
    public const string TypeUGDT = "ugdt";

    public uint LookupHash { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }

    public LocaleDataEntry(uint lookupHash, string type, string content)
    {
        LookupHash = lookupHash;
        Type = type;
        Content = content;
    }

    public static LocaleDataEntry CreateFromLocaleStringId(uint localeStringId, string content, string type = TypeUCDT)
    {
        uint lookupId = Jenkins.LocaleStringIdToLookup(localeStringId);
        return new LocaleDataEntry(lookupId, type, content);
    }

    public static bool TryParse(ReadOnlySpan<char> entryLine, [NotNullWhen(true)] out LocaleDataEntry? entry)
    {
        entry = null;
        SpanReader<char> reader = new(entryLine);

        if (!reader.TryReadTo(out ReadOnlySpan<char> lookupHashStr, '\t'))
            return false;
        if (!uint.TryParse(lookupHashStr, out uint lookupHash))
            return false;

        if (!reader.TryReadTo(out ReadOnlySpan<char> typeStr, '\t'))
            return false;

        // Minor memory optimization so we aren't creating thousands of type objects
        string type;
        if (typeStr.SequenceEqual(TypeUCDT))
            type = TypeUCDT;
        else if (typeStr.SequenceEqual(TypeUGDT))
            type = TypeUGDT;
        else
            type = new string(typeStr);

        if (!reader.TryReadExact(out ReadOnlySpan<char> contentStr, reader.Remaining))
            return false;

        entry = new LocaleDataEntry(lookupHash, type, new string(contentStr));
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{LookupHash}\t{Type}\t{Content}";
}
