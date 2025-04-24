using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Common;

/// <summary>
/// Represents a four-channel color in the order RGBA.
/// </summary>
/// <param name="R">The value of the red channel.</param>
/// <param name="G">The value of the green channel.</param>
/// <param name="B">The value of the blue channel.</param>
/// <param name="Alpha">The value of the alpha channel.</param>
public record struct ColorRGBA(byte R, byte G, byte B, byte Alpha)
{
    /// <summary>
    /// Gets the size consumed by a <see cref="ColorRGBA"/> when serialized.
    /// </summary>
    public const int Size = sizeof(byte) * 4;

    /// <summary>
    /// Reads a <see cref="ColorRGBA"/> instance from a <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="ColorRGBA"/> instance.</returns>
    public static ColorRGBA Read(ref BinaryReader reader)
    {
        byte r = reader.ReadByte();
        byte g = reader.ReadByte();
        byte b = reader.ReadByte();
        byte a = reader.ReadByte();

        return new ColorRGBA(r, g, b, a);
    }

    /// <summary>
    /// Writes this <see cref="ColorRGBA"/> instance to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if the writer does not have enough remaining space.
    /// </exception>
    public void Write(ref BinaryWriter writer)
    {
        if (writer.RemainingLength < Size)
            throw new InvalidBufferSizeException(Size, writer.RemainingLength);

        writer.WriteByte(R);
        writer.WriteByte(G);
        writer.WriteByte(B);
        writer.WriteByte(Alpha);
    }
}
