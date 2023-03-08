using MemoryReaders;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Mandible.Locale;

/// <summary>
/// Represents an entry from a locale data file.
/// </summary>
public class LocaleDataEntry
{
    /// <summary>
    /// Gets or sets the Jenkin's Lookup hash of the data entry.
    /// </summary>
    public uint LookupHash { get; set; }

    /// <summary>
    /// Gets or sets the type of the data entry.
    /// </summary>
    public LocaleDataType Type { get; set; }

    /// <summary>
    /// Gets or sets the content of the data entry.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocaleDataEntry"/> class.
    /// </summary>
    /// <param name="lookupHash">The Jenkin's lookup hash.</param>
    /// <param name="type">The type.</param>
    /// <param name="content">The content.</param>
    public LocaleDataEntry(uint lookupHash, LocaleDataType type, string content)
    {
        LookupHash = lookupHash;
        Type = type;
        Content = content;
    }

    /// <summary>
    /// Attempts to parse a <see cref="LocaleDataEntry"/> object from a string.
    /// </summary>
    /// <param name="entry">The entry string.</param>
    /// <param name="result">The parsed object, or <c>null</c> if parsing failed.</param>
    /// <returns><c>True</c> if parsing was successful, otherwise <c>false</c>.</returns>
    public static bool TryParse(ReadOnlySpan<char> entry, [NotNullWhen(true)] out LocaleDataEntry? result)
    {
        result = null;
        SpanReader<char> reader = new(entry);

        if (!reader.TryReadTo(out ReadOnlySpan<char> lookupHashStr, '\t'))
            return false;
        if (!uint.TryParse(lookupHashStr, out uint lookupHash))
            return false;

        if (!reader.TryReadTo(out ReadOnlySpan<char> typeStr, '\t'))
            return false;

        // Minor memory optimization so we aren't creating thousands of type strings
        LocaleDataType type;
        if (typeStr.SequenceEqual(LocaleDataType.UCDT.TypeValue))
            type = LocaleDataType.UCDT;
        else if (typeStr.SequenceEqual(LocaleDataType.UGDT.TypeValue))
            type = LocaleDataType.UGDT;
        else
            type = new LocaleDataType(new string(typeStr));

        if (!reader.TryReadExact(out ReadOnlySpan<char> contentStr, reader.Remaining))
            return false;

        result = new LocaleDataEntry(lookupHash, type, new string(contentStr));
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{LookupHash}\t{Type}\t{Content}";
}
