using BinaryPrimitiveHelpers;
using Mandible.Common;
using Mandible.Exceptions;

namespace Mandible.Zone;

/// <summary>
/// Information about how to tint an <see cref="EcoLayer"/>.
/// </summary>
/// <param name="Color">The color of the tint.</param>
/// <param name="Strength">The strength of the tint.</param>
public record struct EcoTint(ColorRGBA Color, int Strength)
{
    /// <summary>
    /// The serialized size of this <see cref="EcoTint"/>.
    /// </summary>
    public const int Size = ColorRGBA.Size // Color
        + sizeof(int); // Strength

    /// <summary>
    /// Reads a <see cref="EcoTint"/> instance from a <see cref="BinaryPrimitiveReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>An <see cref="EcoTint"/> instance.</returns>
    public static EcoTint Read(ref BinaryPrimitiveReader reader)
    {
        ColorRGBA color = ColorRGBA.Read(ref reader);
        int strength = reader.ReadInt32LE();

        return new EcoTint(color, strength);
    }

    /// <summary>
    /// Writes this <see cref="EcoTint"/> instance to a <see cref="BinaryPrimitiveWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if there is not enough space remaining in the writer.
    /// </exception>
    public void Write(ref BinaryPrimitiveWriter writer)
    {
        if (Size > writer.RemainingLength)
            throw new InvalidBufferSizeException(Size, writer.RemainingLength);

        Color.Write(ref writer);
        writer.WriteInt32LE(Strength);
    }
}
