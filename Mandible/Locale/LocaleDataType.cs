namespace Mandible.Locale;

/// <summary>
/// Represents a type of locale data.
/// </summary>
/// <param name="TypeValue">The name of the type.</param>
public readonly record struct LocaleDataType(string TypeValue)
{
    /// <summary>
    /// It is unknown what this type represents.
    /// </summary>
    public static readonly LocaleDataType UCDT = new("ucdt");

    /// <summary>
    /// It is unknown what this type represents.
    /// </summary>
    public static readonly LocaleDataType UGDT = new("ugdt");

    /// <inheritdoc />
    public override string ToString()
        => TypeValue;
}
