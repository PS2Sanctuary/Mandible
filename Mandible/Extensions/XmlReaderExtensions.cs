using System;
using System.Xml;

namespace Mandible.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="XmlReader"/> class.
/// </summary>
public static class XmlReaderExtensions
{
    /// <summary>
    /// Attempts to get the value of an attribute containing a boolean value.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <param name="truthyValue">The expected truthy value of the attribute</param>
    /// <returns>
    /// The value of the <paramref name="attributeName"/>, or <c>null</c> if it did not exist.
    /// </returns>
    public static bool? GetOptionalBoolean(this XmlReader reader, string attributeName, string truthyValue = "true")
    {
        string? attribute = reader.GetAttribute(attributeName);
        return attribute is not null
            ? attribute == truthyValue
            : null;
    }

    /// <summary>
    /// Attempts to get the value of an attribute containing an Int32 value.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <returns>
    /// The value of the <paramref name="attributeName"/>, or <c>null</c> if it did not exist.
    /// </returns>
    public static int? GetOptionalInt32(this XmlReader reader, string attributeName)
    {
        string? attribute = reader.GetAttribute(attributeName);
        return attribute is not null
            ? int.Parse(attribute)
            : null;
    }

    /// <summary>
    /// Attempts to get the value of an attribute containing a UInt32 value.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <returns>
    /// The value of the <paramref name="attributeName"/>, or <c>null</c> if it did not exist.
    /// </returns>
    public static uint? GetOptionalUInt32(this XmlReader reader, string attributeName)
    {
        string? attribute = reader.GetAttribute(attributeName);
        return attribute is not null
            ? uint.Parse(attribute)
            : null;
    }

    /// <summary>
    /// Attempts to get the value of an attribute containing a unix seconds timestamp.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <returns>
    /// The timestamp converted to a <see cref="DateTimeOffset"/> value, or <c>null</c>
    /// if the <paramref name="attributeName"/> did not exist.
    /// </returns>
    public static DateTimeOffset? GetOptionalTimestamp(this XmlReader reader, string attributeName)
    {
        string? attribute = reader.GetAttribute(attributeName);
        return attribute is not null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(attribute))
            : null;
    }

    /// <summary>
    /// Gets the value of a required attribute containing an Int32 value.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <returns>The value of the <paramref name="attributeName"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown if the <paramref name="attributeName"/> does not exist.
    /// </exception>
    public static int GetRequiredInt32(this XmlReader reader, string attributeName)
    {
        string? attribute = reader.GetAttribute(attributeName);
        return attribute is not null
            ? int.Parse(attribute)
            : throw new FormatException($"Element did not contain a {attributeName} attribute");
    }

    /// <summary>
    /// Gets the value of a required attribute containing a unix seconds timestamp.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <returns>
    /// The timestamp converted to a <see cref="DateTimeOffset"/> value.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown if the <paramref name="attributeName"/> does not exist.
    /// </exception>
    public static DateTimeOffset GetRequiredTimestamp(this XmlReader reader, string attributeName)
    {
        string? attribute = reader.GetAttribute(attributeName);
        return attribute is not null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(attribute))
            : throw new FormatException($"Element did not contain a {attributeName} attribute");
    }

    /// <summary>
    /// Gets the value of a required attribute.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="attributeName">The name of the attribute to retrieve.</param>
    /// <returns>The value of the <paramref name="attributeName"/>.</returns>
    /// <exception cref="FormatException">
    /// Thrown if the <paramref name="attributeName"/> does not exist.
    /// </exception>
    public static string GetRequiredAttribute(this XmlReader reader, string attributeName)
        => reader.GetAttribute(attributeName)
            ?? throw new FormatException($"Element did not contain a {attributeName} attribute");
}
