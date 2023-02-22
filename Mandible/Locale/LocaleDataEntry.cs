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
    /// Gets the string representation of the UCDT data entry type.
    /// </summary>
    public const string TypeUCDT = "ucdt";

    /// <summary>
    /// Gets the string representation of the UGDT data entry type.
    /// </summary>
    public const string TypeUGDT = "ugdt";

    /// <summary>
    /// Gets or sets the Jenkin's Lookup hash of the data entry.
    /// </summary>
    public uint LookupHash { get; set; }

    /// <summary>
    /// Gets or sets the type of the data entry.
    /// </summary>
    public string Type { get; set; }

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
    public LocaleDataEntry(uint lookupHash, string type, string content)
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

        result = new LocaleDataEntry(lookupHash, type, new string(contentStr));
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{LookupHash}\t{Type}\t{Content}";
}
