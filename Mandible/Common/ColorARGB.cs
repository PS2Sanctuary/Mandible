using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Common;

/// <summary>
/// Represents a four-channel color in the order ARGB.
/// </summary>
/// <param name="Alpha">The value of the alpha channel.</param>
/// <param name="R">The value of the red channel.</param>
/// <param name="G">The value of the green channel.</param>
/// <param name="B">The value of the blue channel.</param>
public record struct ColorARGB(byte Alpha, byte R, byte G, byte B)
{
    /// <summary>
    /// Gets the size consumed by a <see cref="ColorARGB"/> when serialized.
    /// </summary>
    public const int Size = sizeof(byte) * 4;

    /// <summary>
    /// Reads a <see cref="ColorARGB"/> instance from a <see cref="BinaryPrimitiveReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="ColorRGBA"/> instance.</returns>
    public static ColorARGB Read(ref BinaryPrimitiveReader reader)
    {
        byte alpha = reader.ReadByte();
        byte r = reader.ReadByte();
        byte g = reader.ReadByte();
        byte b = reader.ReadByte();

        return new ColorARGB(alpha, r, g, b);
    }

    /// <summary>
    /// Writes this <see cref="ColorARGB"/> instance to a <see cref="BinaryPrimitiveWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <returns></returns>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if the writer does not have enough remaining space.
    /// </exception>
    public void Write(ref BinaryPrimitiveWriter writer)
    {
        if (writer.RemainingLength < Size)
            throw new InvalidBufferSizeException(Size, writer.RemainingLength);

        writer.WriteByte(Alpha);
        writer.WriteByte(R);
        writer.WriteByte(G);
        writer.WriteByte(B);
    }
}
